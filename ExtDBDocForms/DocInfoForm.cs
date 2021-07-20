using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.ExtDB;
using AgeyevAV.Remoting;
using System.Xml;
using AgeyevAV.Config;

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

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// Окно просмотра информации о документе
  /// </summary>
  internal partial class DocInfoForm : Form
  {
    #region Конструктор формы

    public DocInfoForm(DocInfoReport report)
    {
#if DEBUG
      if (report == null)
        throw new ArgumentNullException("report");
#endif

      InitializeComponent();
      _Report = report;
    }

    public DocInfoReport Report { get { return _Report; } }
    private DocInfoReport _Report;

    public DocInfoReportParams Params { get { return _Report.Params; } }

    public DBUI UI { get { return Report.UI; } }

    #endregion

    #region Вкладка "Информация о документе"

    public void InitMainPage(EFPBaseProvider baseProvider)
    {
      #region Свойства документа

      efpDocText = new EFPTextBox(baseProvider, edDocText);
      efpDocText.ReadOnly = true;
      efpDocText.DisplayName = "Текстовое представление";
      efpDocText.ToolTipText = "Представление документа в виде строки текста." + Environment.NewLine +
        "Используется для отображения выбранного документа в комбоблоках, когда выпадающий список закрыт";

      efpId = new EFPTextBox(baseProvider, edId);
      efpId.ReadOnly = true;
      //efpId.CanBeEmpty = false;
      efpId.DisplayName = "Id";
      efpId.ToolTipText = "Внутренний идентификатор строки документа в таблице базы данных." + Environment.NewLine +
        "Генерируется автоматически при создании документа и никогда не изменяется." + Environment.NewLine +
        "Каждый документ данного типа имеет уникальный идентификатор";

      efpStatus = new EFPTextBox(baseProvider, edStatus);
      efpStatus.ReadOnly = true;
      efpStatus.DisplayName = "Статус";
      efpStatus.ToolTipText = "Может принимать значения \"Действует\" или \"Удален\"" + Environment.NewLine +
        "Удаленные документы не показываются в справочниках, если не включен режим \"Показывать удаленные документы\"." + Environment.NewLine +
        "Чтобы восстановить удаленный документ, нужно открыть его на редактирование и сохранить";

      efpVersion = new EFPTextBox(baseProvider, edVersion);
      efpVersion.ReadOnly = true;
      efpVersion.DisplayName = "Версия";
      efpVersion.ToolTipText = "Версия изменения документа." + Environment.NewLine +
        "После создания документа он имеет номер версии \"1\", который затем увеличивается при каждом редактировании или другом изменении документа." + Environment.NewLine +
        "Подробнее можно посмотреть в группе \"История\"";

      efpCreateTime = new EFPTextBox(baseProvider, edCreateTime);
      efpCreateTime.ReadOnly = true;
      efpCreateTime.DisplayName = "Время создания";
      efpCreateTime.ToolTipText = "Дата и время создания документа";

      efpCreateUser = new EFPTextBox(baseProvider, edCreateUser);
      efpCreateUser.ReadOnly = true;
      efpCreateUser.DisplayName = "Кто создал";
      efpCreateUser.ToolTipText = "Пользователь, который создал документ";

      efpChangeTime = new EFPTextBox(baseProvider, edChangeTime);
      efpChangeTime.ReadOnly = true;
      efpChangeTime.DisplayName = "Время последнего изменения";
      efpChangeTime.ToolTipText = "Дата и время последнего изменения документа." + Environment.NewLine +
        "Если после создания документ не редактировался и не изменялся другими способами, то поле пустое." + Environment.NewLine +
        "Подробнее можно посмотреть в группе \"История\"";

      efpChangeUser = new EFPTextBox(baseProvider, edChangeUser);
      efpChangeUser.ReadOnly = true;
      efpChangeUser.DisplayName = "Кто изменил";
      efpChangeUser.ToolTipText = "Пользователь, который последним редактировал документ или выполнил команду, которая привела к изменению документа" + Environment.NewLine +
        "Если после создания документ не изменялся, то поле пустое" + Environment.NewLine +
        "Подробнее можно посмотреть в группе \"История\"";

      if (UI.DebugShowIds)
      {
        btnDebug.Visible = true;
        btnDebug.Image = EFPApp.MainImages.Images["Debug"];
        btnDebug.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpDebug = new EFPButton(baseProvider, btnDebug);
        efpDebug.DisplayName = "Отладочная информация";
        efpDebug.ToolTipText = "Показывает неотформатированные таблицы данных для документа";
        efpDebug.Click += efpDebug_Click;
      }

      #endregion

      #region История изменений

      EFPGridProducer producer = new EFPGridProducer();
      producer.Columns.AddUserImage("Image", "Action", ImageColumnValueNeeded, String.Empty);
      producer.Columns.LastAdded.DisplayName = "Значок действия (создание, изменение, удаление)";
      producer.Columns.AddInt("Version", "Версия документа", 3);
      producer.Columns.AddDateTime("UserActionId.StartTime", "Время начала редактирования");
      producer.Columns.AddDateTime("UserActionId.ActionTime", "Время первого сохранения");
      producer.Columns.AddInt("UserActionId.ApplyChangesCount", "Кол-во сохранений", 4);
      producer.Columns.AddDateTime("UserActionId.ApplyChangesTime", "Время последнего сохранения");
      producer.Columns.AddUserText("EditTime", "UserActionId.StartTime,UserActionId.ApplyChangesTime",
        EditTimeColumnValueNeeded, "Время редактирования", 10, 6);
      producer.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      if (UI.DocProvider.DocTypes.UseUsers)
      {
        producer.Columns.AddUserText("UserActionId.UserId.UserName", "UserActionId.UserId",
          UserNameColumnValueNeeded, "Пользователь", 20, 10);
        producer.Columns.AddInt("UserActionId.UserId", "UserId", 7);
      }
      if (UI.DocProvider.DocTypes.UseSessionId)
        producer.Columns.AddInt("UserActionId.SessionId", "SessionId", 7);
      producer.Columns.AddInt("UserActionId", "UserActionId", 7);
      producer.Columns.AddText("UserActionId.ActionInfo", "Действие пользователя", 100, 20);
      producer.Columns.AddInt("Id", "DocActionId", 7);

      producer.ToolTips.AddDateTime("UserActionId.StartTime", "Время начала редактирования");
      producer.ToolTips.AddDateTime("UserActionId.ActionTime", "Время первого сохранения");
      producer.ToolTips.AddText("UserActionId.ApplyChangesCount", "Кол-во сохранений");
      producer.ToolTips.AddDateTime("UserActionId.ApplyChangesTime", "Время последнего сохранения");
      producer.ToolTips.AddUserItem("EditTime", "UserActionId.StartTime,UserActionId.ApplyChangesTime",
        EditTimeColumnValueNeeded);
      producer.ToolTips.LastAdded.DisplayName = "Время редактирования";
      producer.ToolTips.LastAdded.PrefixText = "Время редактирования: ";
      producer.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      producer.ToolTips.AddText("UserActionId.ActionInfo", String.Empty);
      producer.ToolTips.LastAdded.DisplayName = "Действие пользователя";

      producer.DefaultConfig = new EFPDataGridViewConfig();
      producer.DefaultConfig.Columns.Add("Image");
      producer.DefaultConfig.Columns.Add("Version");
      producer.DefaultConfig.Columns.Add("UserActionId.StartTime");
      producer.DefaultConfig.Columns.Add("UserActionId.ActionTime");
      producer.DefaultConfig.Columns.Add("UserActionId.ApplyChangesCount");
      producer.DefaultConfig.Columns.Add("UserActionId.ApplyChangesTime");
      producer.DefaultConfig.Columns.Add("EditTime");
      if (UI.DocProvider.DocTypes.UseUsers)
        producer.DefaultConfig.Columns.Add("UserActionId.UserId.UserName");
      //if (UI.DocProvider.DocTypes.UseSessionId)
      //  producer.DefaultConfig.Columns.Add("UserActionId.SessionId");
      //producer.DefaultConfig.Columns.Add("UserActionId");
      producer.DefaultConfig.Columns.AddFill("UserActionId.ActionInfo");
      //producer.DefaultConfig.Columns.Add("Id");

      EFPControlWithToolBar<DataGridView> cwtHist = new EFPControlWithToolBar<DataGridView>(baseProvider, grpHist);
      efpHist = new EFPDBxGridView(cwtHist, UI);

      efpHist.Control.AutoGenerateColumns = false;
      efpHist.GridProducer = producer;
      efpHist.ConfigSectionName = "DocInfo_History";
      efpHist.DisableOrdering();

      efpHist.Control.ShowCellToolTips = true;
      efpHist.ReadOnly = true;
      efpHist.Control.ReadOnly = true;
      efpHist.Control.MultiSelect = true; // 28.04.2018
      efpHist.CanView = true;
      efpHist.EditData += new EventHandler(efpHist_EditData);
      efpHist.CommandItems.UseRefresh = false;
      if (UI.DocProvider.DocTypes.UseUsers)
        efpHist.GetDocSel += new EFPDBxGridViewDocSelEventHandler(efpHist_GetDocSel); // 28.04.2018

      EFPCommandItem ci;
      ci = new EFPCommandItem("View", "DataSetXML");
      ci.MenuText = "Данные документа DataSet XML";
      ci.ImageKey = "XMLDataSet";
      ci.Click += ciDataSetXml_Click;
      ci.GroupBegin = true;
      ci.GroupEnd = true;
      efpHist.CommandItems.Add(ci);

      /*
      Args.ControlProvider.ConfigSectionName = "DocInfoReport_History";
      Args.ControlProvider.GridPageSetup.DocumentName = "История документа \"" + Params.DocType.SingularTitle + "\", Id=" + Params.DocId.ToString();
      Args.ControlProvider.GridPageSetup.DocumentName += Environment.NewLine + Params.DocType.GetTextValue(Params.DocId);
      Args.ControlProvider.GridPageSetup.Title = Args.ControlProvider.GridPageSetup.DocumentName;
      Args.ControlProvider.GridPageSetup.InitDefaults += new EventHandler(HistGridPageSetup_InitDefaults);
      Args.ControlProvider.GridPageSetup.BeforePrinting += new CancelEventHandler(HistGridPageSetup_BeforePrinting);
      */

      #endregion
    }

    #region Свойства документа

    public EFPTextBox efpDocText, efpId, efpStatus, efpVersion;

    public EFPTextBox efpCreateTime, efpCreateUser;
    public EFPTextBox efpChangeTime, efpChangeUser;

    void efpDebug_Click(object sender, EventArgs args)
    {
      DataSet ds = UI.DocProvider.LoadUnformattedDocData(Params.DocTypeName, Params.DocId);
      DebugTools.DebugDataSet(ds, "Все таблицы для документа");
    }

    #endregion

    #region Таблица истории

    public EFPDBxGridView efpHist;

    #region Вычисляемые столбцы для GridProducer

    private void ImageColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      UndoAction Action = (UndoAction)(args.GetInt("Action"));
      string ImageKey = DBUI.GetUndoActionImageKey(Action);
      args.Value = EFPApp.MainImages.Images[ImageKey];
      args.ToolTipText = DBUI.GetUndoActionName(Action);
    }

    private void EditTimeColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateTime? StartTime = args.GetNullableDateTime("UserActionId.StartTime");
      DateTime? ApplyChangesTime = args.GetNullableDateTime("UserActionId.ApplyChangesTime");
      if (StartTime.HasValue && ApplyChangesTime.HasValue)
        args.Value = ApplyChangesTime.Value - StartTime.Value;
    }

    private void UserNameColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 UserId = args.GetInt("UserActionId.UserId");
      if (UserId == 0)
        args.Value = "[ нет ]";
      else
        args.Value = UI.TextHandlers.GetTextValue(UI.DocProvider.DocTypes.UsersTableName, UserId);
    }

    #endregion

    void efpHist_EditData(object sender, EventArgs args)
    {
      if (!efpHist.CheckSingleRow())
        return;
      int Version = DataTools.GetInt(efpHist.CurrentDataRow, "Version");

      DBxDocSet DocSet = new DBxDocSet(UI.DocProvider);
      DocSet[Params.DocTypeName].ViewVersion(Params.DocId, Version);
      DocumentEditor de = new DocumentEditor(UI, DocSet);
      de.Run();
    }

    void efpHist_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      args.AddFromColumn(UI.DocProvider.DocTypes.UsersTableName, "UserActionId.UserId");
    }

    #region Команды локального меню

    void ciDataSetXml_Click(object sender, EventArgs args)
    {
      if (!efpHist.CheckSingleRow())
        return;

      int Version = DataTools.GetInt(efpHist.CurrentDataRow, "Version");
      DBxDocSet DocSet = new DBxDocSet(UI.DocProvider);
      DocSet[Params.DocTypeName].ViewVersion(Params.DocId, Version);
      // 06.04.2020
      // Загружаем все поддокументы, иначе их не будет в DataSet'е
      DocSet[Params.DocTypeName].SubDocs.LoadAll();

      string s = DocSet.DataSet.GetXml();
      XmlDocument XmlDoc = DataTools.XmlDocumentFromString(s);
      EFPApp.ShowXmlView(XmlDoc,
        Params.DocTypeUI.DocType.SingularTitle + ", Id=" + Params.DocId.ToString() + ", Version=" + Version.ToString(),
        false,
        Params.DocTypeName + "-Id" + Params.DocId.ToString() + "-v" + Version.ToString() + ".xml");
    }

    #endregion

    #endregion

    #endregion

    #region Вкладка "Ссылки"

    public void InitRefPage(EFPBaseProvider baseProvider)
    {
      efpRefStat = new EFPDataGridView(baseProvider, grRefStat);
      efpRefStat.Control.AutoGenerateColumns = false;
      efpRefStat.Columns.AddImage();
      efpRefStat.Columns.AddTextFill("FromDocTypeText", true, "Вид документа", 100, 10);
      if (UI.DebugShowIds)
      {
        efpRefStat.Columns.AddInt("FromDocTableId", true, "TableId", 4);
      }
      efpRefStat.DisableOrdering();
      efpRefStat.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpRefStat_GetCellAttributes);
      efpRefStat.ReadOnly = true;
      efpRefStat.CanView = false;
      efpRefStat.Control.ReadOnly = true;
      efpRefStat.Control.MultiSelect = false;
      efpRefStat.Control.CurrentCellChanged += new EventHandler(grRefStat_CurrentCellChanged);
      efpRefStat.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.PrimaryKey;
      efpRefStat.CommandItems.UseRefresh = false;

      #region Локальное меню

      efpRefStat.CommandItems.ClearUsage(EFPCommandItemUsage.ToolBar); // оставим только 2 свои кнопки

      ciShowHiddenRefs = new EFPCommandItem("View", "ShowHiddenDocs");
      ciShowHiddenRefs.MenuText = "Показать удаленные документы";
      ciShowHiddenRefs.ImageKey = "ShowHiddenDocs";
      ciShowHiddenRefs.Click += new EventHandler(ciShowHiddenRefs_Click);
      ciShowHiddenRefs.GroupBegin = true;
      efpRefStat.CommandItems.Add(ciShowHiddenRefs);

      EFPCommandItem ciRefInfo = new EFPCommandItem("View", "RefInfo");
      ciRefInfo.MenuText = "Схема (возможные ссылки)";
      // Поле Params может быть еще не инициализировано
      //ciRefInfo.ToolTipText = "Показывает поля в других документах, которые могут ссылаться на документы \"" + Params.DocTypeUI.DocType.PluralTitle + "\"";
      ciRefInfo.ToolTipText = "Показывает поля в других документах, которые могут ссылаться на документы";
      ciRefInfo.ImageKey = "DocRefSchema";
      ciRefInfo.Click += new EventHandler(ciRefInfo_Click);
      ciRefInfo.GroupEnd = true;
      efpRefStat.CommandItems.Add(ciRefInfo);

      efpRefStat.ToolBarPanel = panSpbRefStat;

      #endregion

      efpRefDet = new EFPDBxGridView(baseProvider, grRefDet, UI);
      efpRefDet.Control.AutoGenerateColumns = false;
      efpRefDet.Columns.AddImage();
      efpRefDet.Columns.AddTextFill("FromDocText", false, "Документ", 100, 10);
      efpRefDet.Columns.LastAdded.CanIncSearch = true;
      if (UI.DebugShowIds)
      {
        efpRefDet.Columns.AddInt("FromDocId", true, "DocId", 6);
        efpRefDet.Columns.LastAdded.CanIncSearch = true;
      }
      efpRefDet.DisableOrdering();
      efpRefDet.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpRefDet_GetCellAttributes);
      //efpRefDet.ReadOnly = false;
      efpRefDet.CanInsert = false;
      efpRefDet.CanDelete = false;
      //efpRefDet.CanView = true;
      efpRefDet.Control.ReadOnly = true;
      efpRefDet.Control.MultiSelect = true;
      efpRefDet.EditData += new EventHandler(efpRefDet_EditData);
      efpRefDet.GetDocSel += new EFPDBxGridViewDocSelEventHandler(efpRefDet_GetDocSel);
      // efpRefDet.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.DataViewSort;

      #region Локальное меню

      EFPCommandItem ciSingleRef = new EFPCommandItem("View", "SingleRef");
      ciSingleRef.MenuText = "Детальная информация о ссылке";
      ciSingleRef.ShortCut = Keys.F4;
      ciSingleRef.ImageKey = "Properties";
      ciSingleRef.Click += new EventHandler(ciSingleRef_Click);
      ciSingleRef.GroupBegin = true;
      efpRefDet.CommandItems.Add(ciSingleRef);

      EFPCommandItem ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
      ciShowDocInfo.MenuText = "Информация о документе";
      ciShowDocInfo.ToolTipText = "Информация о документе";
      ciShowDocInfo.ShortCut = Keys.F12;
      ciShowDocInfo.ImageKey = "Information";
      ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
      ciShowDocInfo.GroupEnd = true;
      efpRefDet.CommandItems.Add(ciShowDocInfo);

      #endregion

      efpRefDet.CommandItems.UseRefresh = false;
      efpRefDet.ToolBarPanel = panSpbRefDet;

      InitRefDetTable();
    }

    #region Таблица статистики

    public EFPDataGridView efpRefStat;

    /// <summary>
    /// Полная таблица ссылок, полученная с сервера
    /// </summary>
    public DataTable RefTable;

    public void InitRefPage()
    {

      EFPDataGridViewSelection OldSelStat = efpRefStat.Selection;

      try
      {
        EFPApp.BeginWait("Получение таблицы ссылок на документ");
        try
        {
          RefTable = Params.DocTypeUI.GetDocRefTable(Params.DocId, ciShowHiddenRefs.Checked, true);
          Int32[] FromDocTableIds = DataTools.GetIdsFromField(RefTable, "FromDocTableId");
          DataTable StatTable = new DataTable();
          StatTable.Columns.Add("FromDocTypeName", typeof(string));
          StatTable.Columns.Add("FromDocTableId", typeof(Int32));
          StatTable.Columns.Add("FromDocTypeText", typeof(string));
          DataTools.SetPrimaryKey(StatTable, "FromDocTableId");
          for (int i = 0; i < FromDocTableIds.Length; i++)
          {
            DBxDocType FromDocType = UI.DocProvider.DocTypes.FindByTableId(FromDocTableIds[i]);
            StatTable.Rows.Add(FromDocType.Name, FromDocType.TableId, FromDocType.PluralTitle);
          }
          StatTable.DefaultView.Sort = "FromDocTypeText";
          efpRefStat.Control.DataSource = StatTable;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка построения списка ссылок");
      }


      efpRefStat.Selection = OldSelStat;
    }

    void efpRefStat_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnIndex == 0)
      {
        if (args.DataRow == null)
          return;
        string DocTypeName = DataTools.GetString(args.DataRow, "FromDocTypeName");

        string ImageKey = null;
        DocTypeUI dtui = UI.DocTypes[DocTypeName];
        if (dtui != null)
          ImageKey = dtui.ImageKey;
        if (String.IsNullOrEmpty(ImageKey))
          args.Value = EFPApp.MainImages.Images["Table"];
        else
          args.Value = EFPApp.MainImages.Images[ImageKey];
      }
    }

    void grRefStat_CurrentCellChanged(object sender, EventArgs args)
    {
      DBxDocType NewDocType = GetCurrFromDocType();
      if (NewDocType == _CurrFromDocType)
        return;

      _CurrFromDocType = NewDocType;
      InitRefDetTable();
    }

    #endregion

    #region Команда "Показывать удаленные документы"


    /// <summary>
    /// Команда "Показывать удаленные документы"
    /// </summary>
    private EFPCommandItem ciShowHiddenRefs;

    void ciShowHiddenRefs_Click(object sender, EventArgs args)
    {
      ciShowHiddenRefs.Checked = !ciShowHiddenRefs.Checked;
      InitRefPage(); // перестраиваем все
    }

    #endregion

    void ciRefInfo_Click(object sender, EventArgs args)
    {
      Report.ShowRefInfo();
    }

    #region Таблица списка документов

    /// <summary>
    /// Текущий вид документа, который выбран в таблице "Статистика".
    /// Может быть null
    /// </summary>
    private DBxDocType _CurrFromDocType;

    private DBxDocType GetCurrFromDocType()
    {
      if (efpRefStat.CurrentDataRow == null)
        return null;
      else
      {
        string DocTypeName = DataTools.GetString(efpRefStat.CurrentDataRow, "FromDocTypeName");
        return UI.DocProvider.DocTypes[DocTypeName];
      }
    }

    EFPDBxGridView efpRefDet;

    private void InitRefDetTable()
    {
      if (_CurrFromDocType == null)
      {
        efpRefDet.Control.DataSource = null;
        grpRefDet.Text = "Вид документа не выбран";
      }
      else
      {
        RefTable.DefaultView.RowFilter = new ValueFilter("FromDocTableId", _CurrFromDocType.TableId).ToString();
        efpRefDet.Control.DataSource = RefTable.DefaultView;

        grpRefDet.Text = _CurrFromDocType.PluralTitle;
        DocTypeUI dtui = UI.DocTypes[_CurrFromDocType.Name];
        efpRefDet.CanMultiEdit = dtui.CanMultiEdit;
        efpRefDet.ReadOnly = dtui.TableMode != DBxAccessMode.Full;
        efpRefDet.CanView = dtui.TableMode != DBxAccessMode.None;
      }
    }

    void efpRefDet_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.DataRow == null)
        return;

      Int32 FromDocTableId = DataTools.GetInt(args.DataRow, "FromDocTableId");
      string FromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(FromDocTableId);
      Int32 FromDocId = DataTools.GetInt(args.DataRow, "FromDocId");
      switch (args.ColumnIndex)
      {
        case 0:
          switch (args.Reason)
          {
            case EFPDataGridViewAttributesReason.View:
              string ImageKey;
              if (DataTools.GetBool(args.DataRow, "IsSameDoc"))
                ImageKey = "DocSelfRef";
              else
                ImageKey = UI.ImageHandlers.GetImageKey(FromDocTypeName, FromDocId);
              args.Value = EFPApp.MainImages.Images[ImageKey];
              break;
            case EFPDataGridViewAttributesReason.ToolTip:
              args.ToolTipText = UI.ImageHandlers.GetToolTipText(FromDocTypeName, FromDocId);
              break;
          }
          break;

        case 1:
          string s = UI.TextHandlers.GetTextValue(FromDocTypeName, FromDocId);
          if (DataTools.GetBool(args.DataRow, "IsSameDoc"))
            s += " (ссылается сам на себя)";
          args.Value = s;
          break;
      }
    }

    void efpRefDet_EditData(object sender, EventArgs args)
    {
      DataRow[] rows = efpRefDet.SelectedDataRows;
      if (rows.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет выбранных документов");
        return;
      }

      Int32 FromDocTableId = DataTools.GetInt(rows[0], "FromDocTableId");
      string FromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(FromDocTableId);
      Int32[] FromDocIds = DataTools.GetIdsFromField(rows, "FromDocId");
      UI.DocTypes[FromDocTypeName].PerformEditing(FromDocIds, efpRefDet.State, false);
    }

    void efpRefDet_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      args.AddFromVTReference("FromDocTableId", "FromDocId", true);
    }

    void ciSingleRef_Click(object sender, EventArgs args)
    {
      if (!efpRefDet.CheckSingleRow())
        return;

      Int32 FromDocTableId = DataTools.GetInt(efpRefDet.CurrentDataRow, "FromDocTableId");
      string FromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(FromDocTableId);
      Int32 FromDocId = DataTools.GetInt(efpRefDet.CurrentDataRow, "FromDocId");
      _Report.ShowSingleRef(FromDocTypeName, FromDocId);
    }

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!efpRefDet.CheckSingleRow())
        return;

      Int32 FromDocTableId = DataTools.GetInt(efpRefDet.CurrentDataRow, "FromDocTableId");
      //string FromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(FromDocTableId);
      Int32 FromDocId = DataTools.GetInt(efpRefDet.CurrentDataRow, "FromDocId");
      UI.DocTypes.FindByTableId(FromDocTableId).ShowDocInfo(FromDocId);
    }

    #endregion

    #endregion

  }

  /// <summary>
  /// Параметры отчета "Информация о документе"
  /// </summary>
  internal class DocInfoReportParams : EFPReportParams
  {
    #region Конструктор

    public DocInfoReportParams(DBUI ui)
    {
      _UI = ui;
    }

    #endregion

    #region Свойства

    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    public string DocTypeName
    {
      get { return _DocTypeName; }
      set { _DocTypeName = value; }
    }
    private string _DocTypeName;

    public DocTypeUI DocTypeUI { get { return _UI.DocTypes[DocTypeName]; } }

    public Int32 DocId
    {
      get { return _DocId; }
      set { _DocId = value; }
    }
    private Int32 _DocId;

    #endregion

    #region Переопределенные методы

    protected override void OnInitTitle()
    {
      base.Title = this.DocTypeUI.DocType.SingularTitle + " Id=" + DocId.ToString() + " - Информация о документе";
    }

    // Чтение и запись параметров нужна для сохранение композиции рабочего стола

    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetString("DocTypeName", DocTypeName);
      cfg.SetInt("DocId", DocId);
    }

    public override void ReadConfig(CfgPart cfg)
    {
      DocTypeName = cfg.GetString("DocTypeName");
      DocId = cfg.GetInt("DocId");
    }

    #endregion
  }

  /// <summary>
  /// Просмотр информации о документе и ссылок на него из других документов
  /// </summary>
  internal class DocInfoReport : EFPReport
  {
    #region Конструктор

    public DocInfoReport(DocTypeUI docTypeUI, Int32 docId)
      : this(docTypeUI.UI)
    {
      DocInfoReportParams Params = new DocInfoReportParams(docTypeUI.UI);
      Params.DocTypeName = docTypeUI.DocType.Name;
      Params.DocId = docId;
      base.ReportParams = Params;
    }

    /// <summary>
    /// Эта версия конструктора используется при восстановление композиции пользовательского интерфейса
    /// </summary>
    /// <param name="ui"></param>
    internal DocInfoReport(DBUI ui)
      : base("DocInfo")
    {
      base.MainImageKey = "Information";
      base.StoreComposition = true; // работает через DBUI

      _UI = ui;

      _ReportForm = new DocInfoForm(this);

      _MainPage = new EFPReportControlPage();
      _MainPage.Title = "Информация о документе";
      _MainPage.ImageKey = "Information";
      _MainPage.Control = _ReportForm.Panel1;
      _ReportForm.InitMainPage(_MainPage.BaseProvider);
      _MainPage.DataQuery += new EventHandler(MainPage_DataQuery);
      Pages.Add(_MainPage);

      _RefPage = new EFPReportControlPage();
      _RefPage.Title = "Ссылки";
      _RefPage.ImageKey = "DocRefs";
      _RefPage.Control = _ReportForm.Panel2;
      _ReportForm.InitRefPage(_RefPage.BaseProvider);
      _RefPage.DataQuery += new EventHandler(RefPage_DataQuery);
      Pages.Add(_RefPage);
    }

    #endregion

    #region Параметры отчета

    internal DocInfoReportParams Params { get { return (DocInfoReportParams)(base.ReportParams); } }

    public DBUI UI { get { return _UI; } }
    private DBUI _UI;


    #endregion

    #region Переопределенные методы

    protected override EFPReportParams CreateParams()
    {
      return new DocInfoReportParams(UI); // 22.11.2018
    }

    protected override void BuildReport()
    {
    }

    #endregion

    #region Вкладка "Информация о документе"

    /// <summary>
    /// Форма, из которой используются элементы Panel1 и Panel2
    /// </summary>
    private DocInfoForm _ReportForm;

    private EFPReportControlPage _MainPage;

    void MainPage_DataQuery(object sender, EventArgs args)
    {
      try
      {
        _ReportForm.efpDocText.Text = UI.TextHandlers.GetTextValue(Params.DocTypeName, Params.DocId);

        string ImageKey = Params.DocTypeUI.GetImageKey(Params.DocId);
        if (String.IsNullOrEmpty(ImageKey))
          ImageKey = "Item";
        _ReportForm.DocImageLabel.Image = EFPApp.MainImages.Images[ImageKey];
      }
      catch (Exception e)
      {
        _ReportForm.efpDocText.Text = "Ошибка: " + e.Message;
      }
      _ReportForm.efpId.Text = Params.DocId.ToString();
      //Form.efpStatus.Text = DataTools.GetString(DstData[0]);

      //NamedValues DocVals = UI.DocProvider.GetServiceValues(Params.DocTypeName, Params.DocId);
      DBxDocServiceInfo Info = UI.DocProvider.GetDocServiceInfo(Params.DocTypeName, Params.DocId, false);

      if (UI.DocProvider.DocTypes.UseDeleted)
        _ReportForm.efpStatus.Text = Info.Deleted ? "Удален" : "Действует";
      else
        _ReportForm.efpStatus.Visible = false;

      if (UI.DocProvider.DocTypes.UseVersions)
        _ReportForm.efpVersion.Text = Info.Version.ToString();
      else
        _ReportForm.efpVersion.Visible = false;

      if (UI.DocProvider.DocTypes.UseTime)
      {
        _ReportForm.efpCreateTime.Text = MyTimeText(Info.CreateTime);
        _ReportForm.efpChangeTime.Text = MyTimeText(Info.ChangeTime);
      }
      else
      {
        _ReportForm.efpCreateTime.Visible = false;
        _ReportForm.efpChangeTime.Visible = false;
      }
      if (UI.DocProvider.DocTypes.UseUsers)
      {
        _ReportForm.efpCreateUser.Text = MyUserText(UI, Info.CreateUserId);
        _ReportForm.efpChangeUser.Text = MyUserText(UI, Info.ChangeUserId);
      }
      else
      {
        _ReportForm.efpCreateUser.Visible = false;
        _ReportForm.efpChangeUser.Visible = false;
      }

      if (UI.DocProvider.UseDocHist)
      {
        DataTable TableHist = UI.DocProvider.GetDocHistTable(Params.DocTypeName, Params.DocId);
        TableHist.DefaultView.Sort = "Version,UserActionId.ActionTime";
        _ReportForm.efpHist.SourceAsDataView = TableHist.DefaultView;
      }
      else
      {
        _ReportForm.grpHist.Visible = false;
      }
    }

    private static string MyTimeText(DateTime? dt)
    {
      if (dt.HasValue)
        return dt.Value.ToString("g");
      else
        return String.Empty;
    }

    private static string MyUserText(DBUI ui, Int32 userId)
    {
      if (userId == 0)
        return String.Empty;
      else
        return ui.TextHandlers.GetTextValue(ui.DocProvider.DocTypes.UsersTableName, userId);
    }

    #endregion

    #region Вкладка "Ссылки"

    private EFPReportControlPage _RefPage;

    void RefPage_DataQuery(object sender, EventArgs args)
    {
      _ReportForm.lblNoRefs.Text = "На документы \"" + Params.DocTypeUI.DocType.PluralTitle + "\" не может быть ссылок из других документов";
      _ReportForm.lblNoRefs.Visible = (Params.DocTypeUI.ToDocTypeRefs.Length == 0);
      _ReportForm.lblUserDocsInfo.Visible = (Params.DocTypeName == UI.DocProvider.DocTypes.UsersTableName);

      _ReportForm.InitRefPage();
    }

    #endregion

    #region Вкладка "Схема"

    public void ShowRefInfo()
    {
      if (base.Pages.FindAndActivate("AllPossibleRefs"))
        return;

      EFPReportGridPage RefInfoPage = new EFPReportGridPage();
      RefInfoPage.Title = "Схема";
      RefInfoPage.ToolTipText = "Возможные ссылки на документы \"" + Params.DocTypeUI.DocType.PluralTitle + "\"";
      RefInfoPage.ImageKey = "DocRefSchema";
      RefInfoPage.InitGrid += new EventHandler(RefInfoPage_InitGrid);
      RefInfoPage.ExtraPageKey = "AllPossibleRefs";
      Pages.Add(RefInfoPage);
    }

    void RefInfoPage_InitGrid(object sender, EventArgs args)
    {
      EFPReportGridPage RefInfoPage = (EFPReportGridPage)sender;
      RefInfoPage.ControlProvider.Control.AutoGenerateColumns = false;
      RefInfoPage.ControlProvider.Columns.AddImage("FromImage");
      RefInfoPage.ControlProvider.Columns.LastAdded.GridColumn.ToolTipText = "Значок вида документа / поддокумента, в котором есть ссылочное поле";
      RefInfoPage.ControlProvider.Columns.AddTextFill("FromDoc", false, "Ссылающийся документ", 100, 10);
      RefInfoPage.ControlProvider.Columns.AddText("FromColumn", false, "Ссылочное поле", 20, 10);
      RefInfoPage.ControlProvider.Columns.AddImage("ToImage");
      RefInfoPage.ControlProvider.Columns.LastAdded.GridColumn.ToolTipText = "Значок вида документа / поддокумента, на который выполняется ссылка";
      RefInfoPage.ControlProvider.Columns.AddText("ToSubDocType", false, "Ссылка на", 20, 10);
      RefInfoPage.ControlProvider.DisableOrdering();
      RefInfoPage.ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ghRefInfo_GetCellAttributes);
      RefInfoPage.ControlProvider.ReadOnly = true;
      RefInfoPage.ControlProvider.CanView = false;
      RefInfoPage.ControlProvider.Control.RowCount = Params.DocTypeUI.ToDocTypeRefs.Length;
    }

    void ghRefInfo_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      DBxDocTypeRefInfo[] a = UI.DocProvider.DocTypes.GetToDocTypeRefs(Params.DocTypeName);
      DBxDocTypeRefInfo Info = a[args.RowIndex];

      DocTypeUI FromDocTypeUI;
      string s;

      switch (args.ColumnIndex)
      {
        case 0: // Значок
          FromDocTypeUI = UI.DocTypes[Info.FromDocType.Name];
          if (Info.FromSubDocType == null)
            args.Value = EFPApp.MainImages.Images[FromDocTypeUI.ImageKey];
          else
            args.Value = EFPApp.MainImages.Images[FromDocTypeUI.SubDocTypes[Info.FromSubDocType.Name].ImageKey];
          break;
        case 1: // Ссылающийся документ
          s = Info.FromDocType.PluralTitle;
          if (Info.FromSubDocType != null)
          {
            s += " / " + Info.FromSubDocType.PluralTitle;
            args.IndentLevel = 1;
          }
          args.Value = s;
          break;
        case 2: // Ссылочное поле
          switch (Info.RefType)
          {
            case DBxDocTypeRefType.Column:
              args.Value = Info.FromColumn.ColumnName;
              break;
            case DBxDocTypeRefType.VTRefernce:
              args.Value = "VT:" + Info.FromVTReference.Name;
              break;
          }
          break;
        case 3:
          if (Info.ToSubDocType == null)
            args.Value = EFPApp.MainImages.Images[Params.DocTypeUI.ImageKey];
          else
            args.Value = EFPApp.MainImages.Images[Params.DocTypeUI.SubDocTypes[Info.ToSubDocType.Name].ImageKey];
          break;

        case 4: // Ссылка на поддокумент
          if (Info.ToSubDocType == null)
            args.Value = "[на документ]";
          else
            args.Value = Info.ToSubDocType.SingularTitle;
          break;
      }
    }

    #endregion

    #region Одиночная ссылка

    public void ShowSingleRef(string fromDocTypeName, Int32 fromDocId)
    {
      string ExtraPageKey = fromDocTypeName + ":" + fromDocId.ToString();
      if (base.Pages.FindAndActivate(ExtraPageKey))
        return;

      DocTypeUI FromDTUI = UI.DocTypes[fromDocTypeName];

      EFPReportDBxGridPage SingleRefPage = new EFPReportDBxGridPage(UI);
      SingleRefPage.Title = fromDocId.ToString();
      if (FromDTUI == Params.DocTypeUI && fromDocId == Params.DocId)
        SingleRefPage.ImageKey = "DocSelfRef";
      else
        SingleRefPage.ImageKey = FromDTUI.GetImageKey(fromDocId);
      SingleRefPage.ToolTipText = FromDTUI.DocType.SingularTitle + " Id=" + fromDocId.ToString() +
        " (" + FromDTUI.GetTextValue(fromDocId) + ")";
      SingleRefPage.ExtraPageKey = ExtraPageKey;
      SingleRefPage.FilterInfo.Add("Ссылки из документа", SingleRefPage.ToolTipText);
      if (EFPApp.ShowListImages)
        SingleRefPage.FilterInfo.LastAdded.ImageKey = FromDTUI.GetImageKey(fromDocId);
      SingleRefPage.InitGrid += new EventHandler(SingleRefPage_InitGrid);
      SingleRefPage.ShowToolBar = true;
      base.Pages.Add(SingleRefPage);

      SingleRefPage.DataSource = UI.DocProvider.GetDocRefTable(Params.DocTypeName, Params.DocId, true, false,
        fromDocTypeName, fromDocId).DefaultView;
    }

    void SingleRefPage_InitGrid(object sender, EventArgs args)
    {
      EFPReportDBxGridPage SingleRefPage = (EFPReportDBxGridPage)sender;
      SingleRefPage.ControlProvider.Control.AutoGenerateColumns = false;
      SingleRefPage.ControlProvider.Columns.AddImage("FromImage");
      SingleRefPage.ControlProvider.Columns.AddTextFill("FromText", false, "Ссылка из", 50, 20);
      SingleRefPage.ControlProvider.Columns.AddText("FromColumnName", true, "Ссылочное поле", 15, 10);
      SingleRefPage.ControlProvider.Columns.AddImage("ToImage");
      SingleRefPage.ControlProvider.Columns.AddTextFill("ToText", false, "Ссылка на", 50, 20);
      SingleRefPage.ControlProvider.DisableOrdering();
      SingleRefPage.ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(SingleRefPage_GetCellAttributes);

      SingleRefPage.ControlProvider.ReadOnly = true;
      SingleRefPage.ControlProvider.CanView = false;
      SingleRefPage.ControlProvider.Control.ReadOnly = true;
      //SingleRefPage.ControlProvider.Control.MultiSelect = true;
      // бессмысленно SingleRefPage.ControlProvider.EditData += new EventHandler(SingleRefPage_EditData);
      //бессмысленно SingleRefPage.ControlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SingleRefPage_GetDocSel);
    }

    void SingleRefPage_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.DataRow == null)
        return;

      switch (args.ColumnIndex)
      {
        case 0: // значок "откуда"
          args.Value = EFPApp.MainImages.Images[GetSingleRefImage(DataTools.GetInt(args.DataRow, "FromDocTableId"),
            DataTools.GetInt(args.DataRow, "FromDocId"),
            DataTools.GetString(args.DataRow, "FromSubDocName"),
            DataTools.GetInt(args.DataRow, "FromSubDocId"),
            DataTools.GetBool(args.DataRow, "FromDeleted"))];
          break;

        case 1: // текст "откуда"
          args.Value = GetSingleRefText(DataTools.GetInt(args.DataRow, "FromDocTableId"),
            DataTools.GetInt(args.DataRow, "FromDocId"),
            DataTools.GetString(args.DataRow, "FromSubDocName"),
            DataTools.GetInt(args.DataRow, "FromSubDocId"));
          break;

        case 3: // значок "куда"
          args.Value = EFPApp.MainImages.Images[GetSingleRefImage(Params.DocTypeUI.DocType.TableId,
            Params.DocId,
            DataTools.GetString(args.DataRow, "ToSubDocName"),
            DataTools.GetInt(args.DataRow, "ToSubDocId"),
            DataTools.GetBool(args.DataRow, "ToSubDocDeleted"))];
          break;

        case 4: // текст "куда"
          args.Value = GetSingleRefText(Params.DocTypeUI.DocType.TableId,
            Params.DocId,
            DataTools.GetString(args.DataRow, "ToSubDocName"),
            DataTools.GetInt(args.DataRow, "ToSubDocId"));
          break;
      }
    }

    private string GetSingleRefImage(Int32 docTableId, Int32 docId, string subDocTypeName, Int32 subDocId, bool deleted)
    {
      if (deleted)
        return "Cancel";

      string DocTypeName = UI.DocProvider.DocTypes.GetTableNameById(docTableId);
      DocTypeUI dtui = UI.DocTypes[DocTypeName];
      if (String.IsNullOrEmpty(subDocTypeName))
        return dtui.GetImageKey(docId);
      else
      {
        SubDocTypeUI sdtui = dtui.SubDocTypes[subDocTypeName];
        return sdtui.GetImageKey(subDocId);
      }
    }

    private string GetSingleRefText(Int32 docTableId, Int32 docId, string subDocTypeName, Int32 subDocId)
    {
      string DocTypeName = UI.DocProvider.DocTypes.GetTableNameById(docTableId);
      DocTypeUI dtui = UI.DocTypes[DocTypeName];
      if (String.IsNullOrEmpty(subDocTypeName))
        return dtui.GetTextValue(docId);
      else
      {
        SubDocTypeUI sdtui = dtui.SubDocTypes[subDocTypeName];
        return sdtui.GetTextValue(subDocId);
      }
    }

    //void SingleRefPage_EditData(object Sender, EventArgs Args)
    //{
    //  EFPDBxGridView gh = (EFPDBxGridView)Sender;
    //  if (!gh.CheckSingleRow())
    //    return;

    //  Int32 FromDocTableId = DataTools.GetInt(gh.CurrentDataRow, "FromDocTableId");
    //  string FromDocTypeName = UI.DocProvider.DocTypes.FindByTableId(FromDocTableId).Name;
    //  Int32 FromDocId = DataTools.GetInt(gh.CurrentDataRow, "FromDocId");
    //  UI.DocTypes[FromDocTypeName].PerformEditing(FromDocId, gh.State == EFPDataGridViewState.View);
    //}

    //void SingleRefPage_GetDocSel(object Sender, EFPDBxGridViewDocSelEventArgs Args)
    //{
    //  Args.AddFromVTReference("FromDocTableId", "FromDocId", true);
    //}

    #endregion

    #region Статический метод запуска

    public static void PerformShow(DocTypeUI docTypeUI, Int32 docId)
    {
      DocInfoReport Report = new DocInfoReport(docTypeUI, docId);
      Report.Run();
    }

    #endregion
  }
}