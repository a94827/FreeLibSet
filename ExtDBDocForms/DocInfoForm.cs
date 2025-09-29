// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using System.Xml;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
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
      efpDocText.DisplayName = Res.DocInfo_Name_DocText;
      efpDocText.ToolTipText = Res.DocInfo_ToolTip_DocText;

      efpId = new EFPTextBox(baseProvider, edId);
      efpId.ReadOnly = true;
      //efpId.CanBeEmpty = false;
      efpId.ToolTipText = Res.DocInfo_ToolTip_Id;

      efpStatus = new EFPTextBox(baseProvider, edStatus);
      efpStatus.ReadOnly = true;
      efpStatus.ToolTipText = Res.DocInfo_ToolTip_Status;

      efpVersion = new EFPTextBox(baseProvider, edVersion);
      efpVersion.ReadOnly = true;
      efpVersion.ToolTipText = Res.DocInfo_ToolTip_Version;

      efpCreateTime = new EFPTextBox(baseProvider, edCreateTime);
      efpCreateTime.ReadOnly = true;
      efpCreateTime.DisplayName = Res.Common_ColTitle_CreateTime;
      efpCreateTime.ToolTipText = Res.DocInfo_ToolTip_CreateTime;

      efpCreateUser = new EFPTextBox(baseProvider, edCreateUser);
      efpCreateUser.ReadOnly = true;
      efpCreateUser.DisplayName = Res.Common_ColTitle_CreateUserName;
      efpCreateUser.ToolTipText = Res.DocInfo_ToolTip_CreateUserName;

      efpChangeTime = new EFPTextBox(baseProvider, edChangeTime);
      efpChangeTime.ReadOnly = true;
      efpChangeTime.DisplayName = Res.Common_ColTitle_ChangeTime;
      efpChangeTime.ToolTipText = Res.DocInfo_ToolTip_ChangeTime;

      efpChangeUser = new EFPTextBox(baseProvider, edChangeUser);
      efpChangeUser.ReadOnly = true;
      efpChangeUser.DisplayName = Res.Common_ColTitle_ChangeUserName;
      efpChangeUser.ToolTipText = Res.DocInfo_ToolTip_ChangeUserName;

      if (UI.DebugShowIds)
      {
        btnDebug.Visible = true;
        btnDebug.Image = EFPApp.MainImages.Images["Debug"];
        btnDebug.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpDebug = new EFPButton(baseProvider, btnDebug);
        efpDebug.DisplayName = Res.DocInfo_Cmd_Debug_DocTables;
        efpDebug.ToolTipText = Res.DocInfo_ToolTip_Debug_DocTables;
        efpDebug.Click += efpDebug_Click;
      }

      #endregion

      #region История изменений

      EFPGridProducer producer = new EFPGridProducer();

      producer.OutItem.Default.PageSetup.SetOrientation(FreeLibSet.Reporting.BROrientation.Landscape, true);

      producer.Columns.AddUserImage("Image", "Action", ImageColumnValueNeeded, String.Empty);
      producer.Columns.LastAdded.DisplayName = Res.Common_Name_ActionImage;
      producer.Columns.AddInteger("Version", Res.Common_ColTitle_DocVersion, 3);
      producer.Columns.AddDateTime("UserActionId.StartTime", Res.Common_ColTitle_ActionStartTime);
      producer.Columns.LastAdded.Printed = false;
      producer.Columns.AddDateTime("UserActionId.ActionTime", Res.Common_ColTitle_ActionTime);
      producer.Columns.LastAdded.Printed = false;
      producer.Columns.AddInteger("UserActionId.ApplyChangesCount", Res.Common_ColTitle_ApplyChangesCount, 4);
      producer.Columns.AddDateTime("UserActionId.ApplyChangesTime", Res.Common_ColTitle_ApplyChangesTime);
      producer.Columns.AddUserText("EditTime", "UserActionId.StartTime,UserActionId.ApplyChangesTime",
        EditTimeColumnValueNeeded, Res.Common_ColTitle_EditTime, 10, 6);
      producer.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      if (UI.DocProvider.DocTypes.UseUsers)
      {
        producer.Columns.AddUserText("UserActionId.UserId.UserName", "UserActionId.UserId",
          UserNameColumnValueNeeded, Res.Common_ColTitle_UserName, 20, 10);
        producer.Columns.AddInteger("UserActionId.UserId", "UserId", 7);
      }
      if (UI.DocProvider.DocTypes.UseSessionId)
        producer.Columns.AddInteger("UserActionId.SessionId", "SessionId", 7);
      producer.Columns.AddInteger("UserActionId", "UserActionId", 7);
      producer.Columns.AddText("UserActionId.ActionInfo", Res.Common_ColTitle_ActionInfo, 100, 20);
      producer.Columns.AddInteger("Id", "DocActionId", 7);

      producer.ToolTips.AddDateTime("UserActionId.StartTime", Res.Common_ColTitle_ActionStartTime);
      producer.ToolTips.AddDateTime("UserActionId.ActionTime", Res.Common_ColTitle_ActionTime);
      producer.ToolTips.AddText("UserActionId.ApplyChangesCount", Res.Common_ColTitle_ApplyChangesCount);
      producer.ToolTips.AddDateTime("UserActionId.ApplyChangesTime", Res.Common_ColTitle_ApplyChangesTime);
      producer.ToolTips.AddUserItem("EditTime", "UserActionId.StartTime,UserActionId.ApplyChangesTime",
        EditTimeColumnValueNeeded);
      producer.ToolTips.LastAdded.DisplayName = Res.Common_ColTitle_EditTime;
      producer.ToolTips.LastAdded.PrefixText = Res.Common_ColTitle_EditTime + ": ";
      producer.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      producer.ToolTips.AddText("UserActionId.ActionInfo", String.Empty);
      producer.ToolTips.LastAdded.DisplayName = Res.Common_ColTitle_ActionInfo;

      producer.DefaultConfig = new EFPDataViewConfig();
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

      EFPControlWithToolBar<DataGridView> cwtHist = new EFPControlWithToolBar<DataGridView>(baseProvider, grpHistory);
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
      ci.MenuText = Res.Common_Cmd_DataSetXML;
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
      EFPCommandItem ci = (EFPCommandItem)sender;
      DataSet ds = UI.DocProvider.LoadUnformattedDocData(Params.DocTypeName, Params.DocId);
      FreeLibSet.Forms.Diagnostics.DebugTools.DebugDataSet(ds, ci.MenuTextWithoutMnemonic);
    }

    #endregion

    #region Таблица истории

    public EFPDBxGridView efpHist;

    #region Вычисляемые столбцы для GridProducer

    private void ImageColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      UndoAction action = (UndoAction)(args.GetInt32("Action"));
      string imageKey = DBUI.GetUndoActionImageKey(action);
      args.Value = EFPApp.MainImages.Images[imageKey];
      args.ToolTipText = DBxDocProvider.GetUndoActionName(action);
    }

    private void EditTimeColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      DateTime? startTime = args.GetNullableDateTime("UserActionId.StartTime");
      DateTime? applyChangesTime = args.GetNullableDateTime("UserActionId.ApplyChangesTime");
      if (startTime.HasValue && applyChangesTime.HasValue)
        args.Value = applyChangesTime.Value - startTime.Value;
    }

    private void UserNameColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 userId = args.GetInt32("UserActionId.UserId");
      if (userId == 0)
        args.Value = Res.Common_Msg_NoUser;
      else
        args.Value = UI.TextHandlers.GetTextValue(UI.DocProvider.DocTypes.UsersTableName, userId);
    }

    #endregion

    void efpHist_EditData(object sender, EventArgs args)
    {
      if (!efpHist.CheckSingleRow())
        return;
      int Version = DataTools.GetInt32(efpHist.CurrentDataRow, "Version");

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

      int version = DataTools.GetInt32(efpHist.CurrentDataRow, "Version");
      DBxDocSet docSet = new DBxDocSet(UI.DocProvider);
      docSet[Params.DocTypeName].ViewVersion(Params.DocId, version);
      // 06.04.2020
      // Загружаем все поддокументы, иначе их не будет в DataSet'е
      docSet[Params.DocTypeName].SubDocs.LoadAll();

      string s = docSet.DataSet.GetXml();
      XmlDocument xmlDoc = XmlTools.XmlDocumentFromString(s);
      EFPApp.ShowXmlView(xmlDoc,
        Params.DocTypeUI.DocType.SingularTitle + ", Id=" + Params.DocId.ToString() + ", Version=" + version.ToString(),
        false,
        Params.DocTypeName + "-Id" + Params.DocId.ToString() + "-v" + version.ToString() + ".xml");
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
      efpRefStat.Columns.AddTextFill("FromDocTypeText", true, Res.Common_ColTitle_DocType, 100, 10);
      if (UI.DebugShowIds)
      {
        efpRefStat.Columns.AddInteger("FromDocTableId", true, "TableId", 4);
      }
      efpRefStat.DisableOrdering();
      efpRefStat.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(efpRefStat_CellInfoNeeded);
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
      ciShowHiddenRefs.MenuText = Res.Cmd_Menu_View_ShowHiddenDocs;
      ciShowHiddenRefs.ImageKey = "ShowHiddenDocs";
      ciShowHiddenRefs.Click += new EventHandler(ciShowHiddenRefs_Click);
      ciShowHiddenRefs.GroupBegin = true;
      efpRefStat.CommandItems.Add(ciShowHiddenRefs);

      EFPCommandItem ciRefInfo = new EFPCommandItem("View", "RefInfo");
      ciRefInfo.MenuText = Res.DocInfo_Menu_View_RefInfo;
      // Поле Params может быть еще не инициализировано
      //ciRefInfo.ToolTipText = "Показывает поля в других документах, которые могут ссылаться на документы \"" + Params.DocTypeUI.DocType.PluralTitle + "\"";
      ciRefInfo.ToolTipText = Res.DocInfo_Menu_ToolTip_RefInfo;
      ciRefInfo.ImageKey = "DocRefSchema";
      ciRefInfo.Click += new EventHandler(ciRefInfo_Click);
      ciRefInfo.GroupEnd = true;
      efpRefStat.CommandItems.Add(ciRefInfo);

      efpRefStat.ToolBarPanel = panSpbRefStat;

      #endregion

      efpRefDet = new EFPDBxGridView(baseProvider, grRefDet, UI);
      efpRefDet.Control.AutoGenerateColumns = false;
      efpRefDet.Columns.AddImage();
      efpRefDet.Columns.AddTextFill("FromDocText", false, Res.DocInfo_ColTitle_FromDocText, 100, 10);
      efpRefDet.Columns.LastAdded.CanIncSearch = true;
      if (UI.DebugShowIds)
      {
        efpRefDet.Columns.AddInteger("FromDocId", true, "DocId", 6);
        efpRefDet.Columns.LastAdded.CanIncSearch = true;
      }
      efpRefDet.DisableOrdering();
      efpRefDet.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(efpRefDet_CellInfoNeeded);
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
      ciSingleRef.MenuText = Res.DocInfo_Menu_View_SingleRef;
      ciSingleRef.ShortCut = Keys.F4;
      ciSingleRef.ImageKey = "Properties";
      ciSingleRef.Click += new EventHandler(ciSingleRef_Click);
      ciSingleRef.GroupBegin = true;
      efpRefDet.CommandItems.Add(ciSingleRef);

      EFPCommandItem ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
      ciShowDocInfo.MenuText = Res.Cmd_Menu_DocInfo;
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

      EFPDataGridViewSelection oldSelStat = efpRefStat.Selection;

      try
      {
        EFPApp.BeginWait(Res.DocInfo_Phase_DocRefs);
        try
        {
          RefTable = Params.DocTypeUI.GetDocRefTable(Params.DocId, ciShowHiddenRefs.Checked, true);
          IIdSet<Int32> fromDocTableIds = IdTools.GetIdsFromColumn<Int32>(RefTable, "FromDocTableId");
          DataTable statTable = new DataTable();
          statTable.Columns.Add("FromDocTypeName", typeof(string));
          statTable.Columns.Add("FromDocTableId", typeof(Int32));
          statTable.Columns.Add("FromDocTypeText", typeof(string));
          DataTools.SetPrimaryKey(statTable, "FromDocTableId");
          foreach (Int32 fromDocTableId in fromDocTableIds)
          {
            DBxDocType fromDocType = UI.DocProvider.DocTypes.FindByTableId(fromDocTableId);
            statTable.Rows.Add(fromDocType.Name, fromDocType.TableId, fromDocType.PluralTitle);
          }
          statTable.DefaultView.Sort = "FromDocTypeText";
          efpRefStat.Control.DataSource = statTable;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }

      efpRefStat.Selection = oldSelStat;
    }

    void efpRefStat_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.ColumnIndex == 0)
      {
        if (args.DataRow == null)
          return;
        string docTypeName = DataTools.GetString(args.DataRow, "FromDocTypeName");

        string imageKey = null;
        DocTypeUI dtui = UI.DocTypes[docTypeName];
        if (dtui != null)
          imageKey = dtui.ImageKey;
        if (String.IsNullOrEmpty(imageKey))
          args.Value = EFPApp.MainImages.Images["Table"];
        else
          args.Value = EFPApp.MainImages.Images[imageKey];
      }
    }

    void grRefStat_CurrentCellChanged(object sender, EventArgs args)
    {
      DBxDocType newDocType = GetCurrFromDocType();
      if (newDocType == _CurrFromDocType)
        return;

      _CurrFromDocType = newDocType;
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
    public DBxDocType CurrFromDocType { get { return _CurrFromDocType; } }
    private DBxDocType _CurrFromDocType;

    private DBxDocType GetCurrFromDocType()
    {
      if (efpRefStat.CurrentDataRow == null)
        return null;
      else
      {
        string docTypeName = DataTools.GetString(efpRefStat.CurrentDataRow, "FromDocTypeName");
        return UI.DocProvider.DocTypes[docTypeName];
      }
    }

    public EFPDBxGridView efpRefDet;

    private void InitRefDetTable()
    {
      if (_CurrFromDocType == null)
      {
        efpRefDet.Control.DataSource = null;
        grpRefDet.Text = Res.DocInfo_Msg_DocTypeNotSelected;
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

    void efpRefDet_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.DataRow == null)
        return;

      Int32 fromDocTableId = DataTools.GetInt32(args.DataRow, "FromDocTableId");
      string fromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(fromDocTableId);
      Int32 fromDocId = DataTools.GetInt32(args.DataRow, "FromDocId");
      switch (args.ColumnIndex)
      {
        case 0:
          switch (args.Reason)
          {
            case EFPDataViewInfoReason.View:
              string imageKey;
              if (DataTools.GetBoolean(args.DataRow, "IsSameDoc"))
                imageKey = "DocSelfRef";
              else
                imageKey = UI.ImageHandlers.GetImageKey(fromDocTypeName, fromDocId);
              args.Value = EFPApp.MainImages.Images[imageKey];
              break;
            case EFPDataViewInfoReason.ToolTip:
              args.ToolTipText = UI.ImageHandlers.GetToolTipText(fromDocTypeName, fromDocId);
              break;
          }
          break;

        case 1:
          string s = UI.TextHandlers.GetTextValue(fromDocTypeName, fromDocId);
          if (DataTools.GetBoolean(args.DataRow, "IsSameDoc"))
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
        EFPApp.ShowTempMessage(Res.Common_ToolTipText_NoDoc);
        return;
      }

      Int32 fromDocTableId = DataTools.GetInt32(rows[0], "FromDocTableId");
      string fromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(fromDocTableId);
      IIdSet<Int32> fromDocIds = IdTools.GetIdsFromColumn<Int32>(rows, "FromDocId");
      UI.DocTypes[fromDocTypeName].PerformEditing(fromDocIds, efpRefDet.State, false);
    }

    void efpRefDet_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      args.AddFromVTReference("FromDocTableId", "FromDocId", true);
    }

    void ciSingleRef_Click(object sender, EventArgs args)
    {
      if (!efpRefDet.CheckSingleRow())
        return;

      Int32 fromDocTableId = DataTools.GetInt32(efpRefDet.CurrentDataRow, "FromDocTableId");
      string fromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(fromDocTableId);
      Int32 fromDocId = DataTools.GetInt32(efpRefDet.CurrentDataRow, "FromDocId");
      _Report.ShowSingleRef(fromDocTypeName, fromDocId);
    }

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!efpRefDet.CheckSingleRow())
        return;

      Int32 fromDocTableId = DataTools.GetInt32(efpRefDet.CurrentDataRow, "FromDocTableId");
      //string FromDocTypeName = UI.DocProvider.DocTypes.GetTableNameById(FromDocTableId);
      Int32 fromDocId = DataTools.GetInt32(efpRefDet.CurrentDataRow, "FromDocId");
      UI.DocTypes.FindByTableId(fromDocTableId).ShowDocInfo(fromDocId);
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
      base.Title = String.Format(Res.DocInfo_Title_Form, this.DocTypeUI.DocType.SingularTitle, DocId);
    }

    // Чтение и запись параметров нужна для сохранение композиции рабочего стола

    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetString("DocTypeName", DocTypeName);
      cfg.SetInt32("DocId", DocId);
    }

    public override void ReadConfig(CfgPart cfg)
    {
      DocTypeName = cfg.GetString("DocTypeName");
      DocId = cfg.GetInt32("DocId");
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
      DocInfoReportParams reportParams = new DocInfoReportParams(docTypeUI.UI);
      reportParams.DocTypeName = docTypeUI.DocType.Name;
      reportParams.DocId = docId;
      base.ReportParams = reportParams;
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
      _MainPage.Title = _ReportForm.tpInfo.Text;
      _MainPage.ImageKey = "Information";
      _MainPage.Control = _ReportForm.Panel1;
      _ReportForm.InitMainPage(_MainPage.BaseProvider);
      _MainPage.DataQuery += new EventHandler(MainPage_DataQuery);
      _ReportForm.efpHist.DefaultOutItem.TitleNeeded += MainPage_DefaultOutItem_TitleNeeded;
      Pages.Add(_MainPage);

      _RefPage = new EFPReportControlPage();
      _RefPage.Title = _ReportForm.tpRefs.Text;
      _RefPage.ImageKey = "DocRefs";
      _RefPage.Control = _ReportForm.Panel2;
      _ReportForm.InitRefPage(_RefPage.BaseProvider);
      _RefPage.DataQuery += new EventHandler(RefPage_DataQuery);
      _ReportForm.efpRefStat.MenuOutItems.Clear();
      _ReportForm.efpRefDet.DefaultOutItem.TitleNeeded += new EventHandler(RefDet_DefaultOutItem_TitleNeeded);
      Pages.Add(_RefPage);
    }

    #endregion

    #region Параметры отчета

    internal DocInfoReportParams Params { get { return (DocInfoReportParams)(base.ReportParams); } }

    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;


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

        string imageKey = Params.DocTypeUI.GetImageKey(Params.DocId);
        if (String.IsNullOrEmpty(imageKey))
          imageKey = "Item";
        _ReportForm.DocImageLabel.Image = EFPApp.MainImages.Images[imageKey];
      }
      catch (Exception e)
      {
        _ReportForm.efpDocText.Text = String.Format(Res.Common_Err_ErrorMessage, e.Message);
      }
      _ReportForm.efpId.Text = Params.DocId.ToString();
      //Form.efpStatus.Text = DataTools.GetString(DstData[0]);

      //NamedValues DocVals = UI.DocProvider.GetServiceValues(Params.DocTypeName, Params.DocId);
      DBxDocServiceInfo info = UI.DocProvider.GetDocServiceInfo(Params.DocTypeName, Params.DocId, false);

      if (UI.DocProvider.DocTypes.UseDeleted)
        _ReportForm.efpStatus.Text = info.Deleted ? Res.DocInfo_Msg_DocStatusDeleted : Res.DocInfo_Msg_DocStatusActive;
      else
        _ReportForm.efpStatus.Visible = false;

      if (UI.DocProvider.DocTypes.UseVersions)
        _ReportForm.efpVersion.Text = info.Version.ToString();
      else
        _ReportForm.efpVersion.Visible = false;

      if (UI.DocProvider.DocTypes.UseTime)
      {
        _ReportForm.efpCreateTime.Text = MyTimeText(info.CreateTime);
        _ReportForm.efpChangeTime.Text = MyTimeText(info.ChangeTime);
      }
      else
      {
        _ReportForm.efpCreateTime.Visible = false;
        _ReportForm.efpChangeTime.Visible = false;
      }
      if (UI.DocProvider.DocTypes.UseUsers)
      {
        _ReportForm.efpCreateUser.Text = MyUserText(UI, info.CreateUserId);
        _ReportForm.efpChangeUser.Text = MyUserText(UI, info.ChangeUserId);
      }
      else
      {
        _ReportForm.efpCreateUser.Visible = false;
        _ReportForm.efpChangeUser.Visible = false;
      }

      if (UI.DocProvider.UseDocHist)
      {
        DataTable tableHist = UI.DocProvider.GetDocHistTable(Params.DocTypeName, Params.DocId);
        tableHist.DefaultView.Sort = "Version,UserActionId.ActionTime";
        _ReportForm.efpHist.SourceAsDataView = tableHist.DefaultView;
      }
      else
      {
        _ReportForm.grpHistory.Visible = false;
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

    private void MainPage_DefaultOutItem_TitleNeeded(object sender, EventArgs args)
    {
      Reporting.BRDataGridViewMenuOutItem outItem = (Reporting.BRDataGridViewMenuOutItem)sender;
      outItem.Title = String.Format(Res.DocInfo_Title_MainOutItem, Params.DocTypeUI.DocType.SingularTitle, Params.DocId);
      outItem.FilterInfo.Clear();
      outItem.FilterInfo.Add(Res.DocInfo_Title_FilterToDoc, UI.TextHandlers.GetTextValue(Params.DocTypeName, Params.DocId));
    }

    #endregion

    #region Вкладка "Ссылки"

    private EFPReportControlPage _RefPage;

    void RefPage_DataQuery(object sender, EventArgs args)
    {
      _ReportForm.lblNoRefs.Text = String.Format(Res.DocInfo_Msg_RefsImpossible, Params.DocTypeUI.DocType.PluralTitle);
      _ReportForm.lblNoRefs.Visible = (Params.DocTypeUI.ToDocTypeRefs.Length == 0);
      _ReportForm.lblUserDocsInfo.Visible = (Params.DocTypeName == UI.DocProvider.DocTypes.UsersTableName);

      _ReportForm.InitRefPage();
    }

    private void RefDet_DefaultOutItem_TitleNeeded(object sender, EventArgs args)
    {
      Reporting.BRDataGridViewMenuOutItem outItem = (Reporting.BRDataGridViewMenuOutItem)sender;
      outItem.Title = String.Format(Res.DocInfo_Title_RefOutItem, Params.DocTypeUI.DocType.SingularTitle, Params.DocId);
      outItem.FilterInfo.Clear();
      outItem.FilterInfo.Add(Res.DocInfo_Title_FilterToDoc, UI.TextHandlers.GetTextValue(Params.DocTypeName, Params.DocId));
      string fromTypeText = Res.DocInfo_Msg_AllTypeDocs;
      if (_ReportForm.CurrFromDocType != null)
        fromTypeText = _ReportForm.CurrFromDocType.PluralTitle;
      outItem.FilterInfo.Add(Res.DocInfo_Title_FilterFromDocs, fromTypeText);
    }

    #endregion

    #region Вкладка "Схема"

    public void ShowRefInfo()
    {
      if (base.Pages.FindAndActivate("AllPossibleRefs"))
        return;

      EFPReportGridPage refInfoPage = new EFPReportGridPage();
      refInfoPage.Title = "Схема";
      refInfoPage.ToolTipText = String.Format(Res.DocInfo_ToolTip_RefInfoPage, Params.DocTypeUI.DocType.PluralTitle);
      refInfoPage.ImageKey = "DocRefSchema";
      refInfoPage.InitGrid += new EventHandler(RefInfoPage_InitGrid);
      refInfoPage.ExtraPageKey = "AllPossibleRefs";
      Pages.Add(refInfoPage);
    }

    void RefInfoPage_InitGrid(object sender, EventArgs args)
    {
      EFPReportGridPage refInfoPage = (EFPReportGridPage)sender;
      refInfoPage.ControlProvider.Control.AutoGenerateColumns = false;
      refInfoPage.ControlProvider.Columns.AddImage("FromImage"); // 0
      refInfoPage.ControlProvider.Columns.LastAdded.GridColumn.ToolTipText = Res.DocInfo_ToolTip_SchemaColFromImage;
      refInfoPage.ControlProvider.Columns.AddTextFill("FromDoc", false, Res.DocInfo_ColTitle_RefFromDoc, 100, 10); // 1
      refInfoPage.ControlProvider.Columns.AddText("FromColumn", false, Res.DocInfo_ColTitle_RefFromColumn, 20, 10); // 2
      refInfoPage.ControlProvider.Columns.AddImage("ToImage"); // 3
      refInfoPage.ControlProvider.Columns.LastAdded.GridColumn.ToolTipText = Res.DocInfo_ToolTip_SchemaColToImage;
      refInfoPage.ControlProvider.Columns.AddText("ToSubDocType", false, Res.DocInfo_ColTitle_SchemaColTo, 20, 10); //
      refInfoPage.ControlProvider.DisableOrdering();
      refInfoPage.ControlProvider.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(ghRefInfo_CellInfoNeeded);
      refInfoPage.ControlProvider.ReadOnly = true;
      refInfoPage.ControlProvider.CanView = false;
      refInfoPage.ControlProvider.Control.RowCount = Params.DocTypeUI.ToDocTypeRefs.Length;
    }

    void ghRefInfo_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      DBxDocTypeRefInfo[] a = UI.DocProvider.DocTypes.GetToDocTypeRefs(Params.DocTypeName);
      DBxDocTypeRefInfo info = a[args.RowIndex];

      DocTypeUI fromDocTypeUI;
      string s;

      switch (args.ColumnIndex)
      {
        case 0: // Значок
          fromDocTypeUI = UI.DocTypes[info.FromDocType.Name];
          if (info.FromSubDocType == null)
            args.Value = EFPApp.MainImages.Images[fromDocTypeUI.ImageKey];
          else
            args.Value = EFPApp.MainImages.Images[fromDocTypeUI.SubDocTypes[info.FromSubDocType.Name].ImageKey];
          break;
        case 1: // Ссылающийся документ
          s = info.FromDocType.PluralTitle;
          if (info.FromSubDocType != null)
          {
            s += " / " + info.FromSubDocType.PluralTitle;
            args.IndentLevel = 1;
          }
          args.Value = s;
          break;
        case 2: // Ссылочное поле
          switch (info.RefType)
          {
            case DBxDocTypeRefType.Column:
              args.Value = info.FromColumn.ColumnName;
              break;
            case DBxDocTypeRefType.VTRefernce:
              args.Value = "VT:" + info.FromVTReference.Name;
              break;
          }
          break;
        case 3:
          if (info.ToSubDocType == null)
            args.Value = EFPApp.MainImages.Images[Params.DocTypeUI.ImageKey];
          else
            args.Value = EFPApp.MainImages.Images[Params.DocTypeUI.SubDocTypes[info.ToSubDocType.Name].ImageKey];
          break;

        case 4: // Ссылка на поддокумент
          if (info.ToSubDocType == null)
            args.Value = "[на документ]";
          else
            args.Value = info.ToSubDocType.SingularTitle;
          break;
      }
    }

    #endregion

    #region Одиночная ссылка

    public void ShowSingleRef(string fromDocTypeName, Int32 fromDocId)
    {
      string extraPageKey = fromDocTypeName + ":" + fromDocId.ToString();
      if (base.Pages.FindAndActivate(extraPageKey))
        return;

      DocTypeUI fromDTUI = UI.DocTypes[fromDocTypeName];

      EFPReportDBxGridPage singleRefPage = new EFPReportDBxGridPage(UI);
      singleRefPage.Title = fromDocId.ToString();
      if (fromDTUI == Params.DocTypeUI && fromDocId == Params.DocId)
        singleRefPage.ImageKey = "DocSelfRef";
      else
        singleRefPage.ImageKey = fromDTUI.GetImageKey(fromDocId);
      singleRefPage.ToolTipText = fromDTUI.DocType.SingularTitle + " Id=" + fromDocId.ToString() +
        " (" + fromDTUI.GetTextValue(fromDocId) + ")";
      singleRefPage.ExtraPageKey = extraPageKey;
      singleRefPage.FilterInfo.Add("Ссылки из документа", singleRefPage.ToolTipText);
      if (EFPApp.ShowListImages)
        singleRefPage.FilterInfo.LastAdded.ImageKey = fromDTUI.GetImageKey(fromDocId);
      singleRefPage.InitGrid += new EventHandler(SingleRefPage_InitGrid);
      singleRefPage.ShowToolBar = true;
      base.Pages.Add(singleRefPage);

      singleRefPage.DataSource = UI.DocProvider.GetDocRefTable(Params.DocTypeName, Params.DocId, true, false,
        fromDocTypeName, fromDocId).DefaultView;
    }

    void SingleRefPage_InitGrid(object sender, EventArgs args)
    {
      EFPReportDBxGridPage singleRefPage = (EFPReportDBxGridPage)sender;
      singleRefPage.ControlProvider.Control.AutoGenerateColumns = false;
      singleRefPage.ControlProvider.Columns.AddImage("FromImage");
      singleRefPage.ControlProvider.Columns.AddTextFill("FromText", false, Res.DocInfo_ColTitle_SchemaColFrom, 50, 20);
      singleRefPage.ControlProvider.Columns.AddText("FromColumnName", true, Res.DocInfo_ColTitle_RefFromColumn, 15, 10);
      singleRefPage.ControlProvider.Columns.AddImage("ToImage");
      singleRefPage.ControlProvider.Columns.AddTextFill("ToText", false, Res.DocInfo_ColTitle_SchemaColTo, 50, 20);
      singleRefPage.ControlProvider.DisableOrdering();
      singleRefPage.ControlProvider.CellInfoNeeded += new EFPDataGridViewCellInfoEventHandler(SingleRefPage_CellInfoNeeded);

      singleRefPage.ControlProvider.ReadOnly = true;
      singleRefPage.ControlProvider.CanView = false;
      singleRefPage.ControlProvider.Control.ReadOnly = true;
      //SingleRefPage.ControlProvider.Control.MultiSelect = true;
      // бессмысленно SingleRefPage.ControlProvider.EditData += new EventHandler(SingleRefPage_EditData);
      //бессмысленно SingleRefPage.ControlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(SingleRefPage_GetDocSel);
    }

    void SingleRefPage_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.DataRow == null)
        return;

      switch (args.ColumnIndex)
      {
        case 0: // значок "откуда"
          args.Value = EFPApp.MainImages.Images[GetSingleRefImage(DataTools.GetInt32(args.DataRow, "FromDocTableId"),
            DataTools.GetInt32(args.DataRow, "FromDocId"),
            DataTools.GetString(args.DataRow, "FromSubDocName"),
            DataTools.GetInt32(args.DataRow, "FromSubDocId"),
            DataTools.GetBoolean(args.DataRow, "FromDeleted"))];
          break;

        case 1: // текст "откуда"
          args.Value = GetSingleRefText(DataTools.GetInt32(args.DataRow, "FromDocTableId"),
            DataTools.GetInt32(args.DataRow, "FromDocId"),
            DataTools.GetString(args.DataRow, "FromSubDocName"),
            DataTools.GetInt32(args.DataRow, "FromSubDocId"));
          break;

        case 3: // значок "куда"
          args.Value = EFPApp.MainImages.Images[GetSingleRefImage(Params.DocTypeUI.DocType.TableId,
            Params.DocId,
            DataTools.GetString(args.DataRow, "ToSubDocName"),
            DataTools.GetInt32(args.DataRow, "ToSubDocId"),
            DataTools.GetBoolean(args.DataRow, "ToSubDocDeleted"))];
          break;

        case 4: // текст "куда"
          args.Value = GetSingleRefText(Params.DocTypeUI.DocType.TableId,
            Params.DocId,
            DataTools.GetString(args.DataRow, "ToSubDocName"),
            DataTools.GetInt32(args.DataRow, "ToSubDocId"));
          break;
      }
    }

    private string GetSingleRefImage(Int32 docTableId, Int32 docId, string subDocTypeName, Int32 subDocId, bool deleted)
    {
      if (deleted)
        return "Cancel";

      string docTypeName = UI.DocProvider.DocTypes.GetTableNameById(docTableId);
      DocTypeUI dtui = UI.DocTypes[docTypeName];
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
      string docTypeName = UI.DocProvider.DocTypes.GetTableNameById(docTableId);
      DocTypeUI dtui = UI.DocTypes[docTypeName];
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

    //  Int32 FromDocTableId = DataTools.GetInt32(gh.CurrentDataRow, "FromDocTableId");
    //  string FromDocTypeName = UI.DocProvider.DocTypes.FindByTableId(FromDocTableId).Name;
    //  Int32 FromDocId = DataTools.GetInt32(gh.CurrentDataRow, "FromDocId");
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
      DocInfoReport report = new DocInfoReport(docTypeUI, docId);
      report.Run();
    }

    #endregion
  }
}
