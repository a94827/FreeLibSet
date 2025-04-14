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
using System.Collections.Specialized;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using FreeLibSet.Calendar;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Форма настройки параметров
  /// </summary>
  internal partial class UserActionsReportParamsForm : Form
  {
    #region Конструктор

    public UserActionsReportParamsForm(DBUI ui)
    {
      InitializeComponent();

      Icon = EFPApp.MainImages.Icons["UserActions"];

      efpForm = new EFPFormProvider(this);

      if (ui.DocProvider.DocTypes.UseUsers)
      {
        efpUser = new EFPDocComboBox(efpForm, cbUser, ui.DocTypes[ui.DocProvider.DocTypes.UsersTableName]);
        efpUser.ToolTipText = Res.UserActionReport_ToolTip_User;
        efpUser.CanBeEmpty = true;
        efpUser.EmptyText = Res.UserActionReport_Msg_AllUsers;
        // TODO: if (!AccDepClientExec.UserIsAdministrator)
        //  efpUser.Enabled = false;
      }
      else // 21.05.2019
      {
        EFPUserSelComboBox efpDummy = new EFPUserSelComboBox(efpForm, cbUser);
        efpDummy.Visible = false;
      }

      efpPeriod = new EFPDateRangeBox(efpForm, edPeriod);
      efpPeriod.First.ToolTipText = Res.UserActionReport_ToolTip_FirstDate;
      efpPeriod.First.CanBeEmpty = true;
      efpPeriod.Last.ToolTipText = Res.UserActionReport_ToolTip_LastDate;
      efpPeriod.Last.CanBeEmpty = true;

      btnLastDay.Image = EFPApp.MainImages.Images["ArrowUpThenLeft"];
      btnLastDay.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpLastDay = new EFPButton(efpForm, btnLastDay);
      efpLastDay.DisplayName = Res.UserActionReport_Name_FindLastDay;
      if (ui.DocProvider.DocTypes.UseUsers)
      {
        efpLastDay.ToolTipText = Res.UserActionReport_ToolTip_FindLastDay;
        efpLastDay.EnabledEx = new DepExpr1<bool, int>(efpUser.DocIdEx, CalcLastDayEnabled);
        efpLastDay.Click += new EventHandler(efpLastDay_Click);
      }
      else
        efpLastDay.Visible = false; // 21.05.2019

      efpOneType = new EFPCheckBox(efpForm, cbOneType);

      efpDocType = new EFPDocTypeComboBox(efpForm, cbDocType, ui, null);
      efpDocType.CanBeEmpty = false;
      efpDocType.EnabledEx = efpOneType.CheckedEx;
    }

    private static bool CalcLastDayEnabled(Int32 userId)
    {
      return userId != 0;
    }

    #endregion

    #region Поля

    public EFPFormProvider efpForm;

    public EFPDocComboBox efpUser;

    public EFPDateRangeBox efpPeriod;

    public EFPCheckBox efpOneType;

    public EFPDocTypeComboBox efpDocType;

    #endregion

    #region Поиск периода

    void efpLastDay_Click(object sender, EventArgs args)
    {
      if (efpUser.DocId == 0)
      {
        efpUser.SetFocus(Res.UserActionReport_Err_UserNotSelected);
        return;
      }

      DateTime? dt = efpUser.UI.DocProvider.GetUserActionsLastTime(efpUser.DocId);
      if (dt.HasValue)
      {
        efpPeriod.First.NValue = dt.Value.Date;
        efpPeriod.Last.NValue = dt.Value.Date;
      }
      else
        efpUser.SetFocus(Res.UserActionReport_Err_UserHasNoAction);

    }

    #endregion
  }

  /// <summary>
  /// Параметры отчета для просмотра действий пользователя.
  /// Заполненные параметры должны быть переданы в метод <see cref="DBUI.ShowUserActions(UserActionsReportParams)"/>.
  /// </summary>
  public class UserActionsReportParams : EFPReportParams
  {
    #region Конструктор

    /// <summary>
    /// Создает параметры со значениями по умолчанию:
    /// - период (<see cref="FirstDate"/>:<see cref="LastDate"/>) равен одному дню - текущей дате;
    /// - пользователь <see cref="UserId"/>=<see cref="DBxDocProvider.UserId"/>;
    /// - <see cref="SingleDocTypeName"/> равен пустой строке.
    /// </summary>
    /// <param name="ui">Интерфейс для доступа к документам. Не может быть null</param>
    public UserActionsReportParams(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");

      _UI = ui;

      FirstDate = DateTime.Today;
      LastDate = FirstDate;
      UserId = _UI.DocProvider.UserId;
      SingleDocTypeName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

    /// <summary>
    /// Начальная дата периода, за который просматривается история.
    /// Допускаются открытые и полуоткрытые интервалы.
    /// </summary>
    public DateTime? FirstDate { get { return _FirstDate; } set { _FirstDate = value; } }
    private DateTime? _FirstDate;

    /// <summary>
    /// Конечная дата периода, за который просматривается история.
    /// Допускаются открытые и полуоткрытые интервалы.
    /// </summary>
    public DateTime? LastDate { get { return _LastDate; } set { _LastDate = value; } }
    private DateTime? _LastDate;

    /// <summary>
    /// Идентификатор пользователя.
    /// Значение 0 означает "Все пользователи".
    /// Если <see cref="DBxDocTypes.UseUsers"/>=false, свойство игнорируется.
    /// </summary>
    public Int32 UserId { get { return _UserId; } set { _UserId = value; } }
    private Int32 _UserId;

    /// <summary>
    /// Если свойство установлено, то будет выведена история только для заданного вида документов.
    /// Если пустая строка, то будут показаны все виды документов.
    /// </summary>
    public string SingleDocTypeName { get { return _SingleDocTypeName; } set { _SingleDocTypeName = value; } }
    private string _SingleDocTypeName;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация заголовка отчета и таблички фильтров
    /// </summary>
    protected override void OnInitTitle()
    {
      FilterInfo.Add("Период", DateRangeFormatter.Default.ToString(FirstDate, LastDate, true));
      if (UI.DocProvider.DocTypes.UseUsers)
      {
        if (UserId == 0)
          FilterInfo.Add(Res.UserActionReport_Name_UserFilter, Res.UserActionReport_Msg_AllUsers);
        else
        {
          FilterInfo.Add(Res.UserActionReport_Name_UserFilter, UI.GetUserName(UserId));
          if (EFPApp.ShowListImages)
            FilterInfo.LastAdded.ImageKey = UI.GetUserImageKey(UserId);
        }
      }
      if (!String.IsNullOrEmpty(SingleDocTypeName))
      {
        FilterInfo.Add("Документы", UI.DocProvider.DocTypes[SingleDocTypeName].PluralTitle);
        if (EFPApp.ShowListImages)
          FilterInfo.LastAdded.ImageKey = UI.DocTypes[SingleDocTypeName].ImageKey;
      }
      Title = Res.UserActionReport_Title_Default;
    }

    /// <summary>
    /// Чтение параметров
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstDate = cfg.GetNullableDate("FirstDate");
      LastDate = cfg.GetNullableDate("LastDate");
      if (UI.DocProvider.DocTypes.UseUsers)
        UserId = cfg.GetInt("UserId");
      SingleDocTypeName = cfg.GetString("DocType");
    }

    /// <summary>
    /// Сохранение параметров
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDate("FirstDate", FirstDate);
      cfg.SetNullableDate("LastDate", LastDate);
      if (UI.DocProvider.DocTypes.UseUsers)
        cfg.SetInt("UserId", UserId);
      cfg.SetString("DocType", SingleDocTypeName);
    }

    #endregion
  }

  internal class UserActionsReport : EFPReport
  {
    #region Конструктор

    public UserActionsReport(DBUI ui)
      : base("UserActions")
    {
      _UI = ui;
      base.MainImageKey = "UserActions";
      base.StoreComposition = true; // работает через DBUI

      if (_DummyManager == null)
        _DummyManager = new EFPRuntimeOnlyConfigManager();
      base.ConfigManager = _DummyManager;

      _MainPage = new EFPReportDBxGridPage(ui);
      _MainPage.Title = Res.UserActionReport_Title_MainPage;
      _MainPage.ToolTipText = Res.UserActionReport_ToolTip_MainPage;
      _MainPage.ImageKey = MainImageKey;
      _MainPage.InitGrid += new EventHandler(MainPage_InitGrid);
      Pages.Add(_MainPage);
    }

    #endregion

    #region Поля

    /// <summary>
    /// Храним параметры отчета только в текущем сеансе работы
    /// </summary>
    private static EFPRuntimeOnlyConfigManager _DummyManager = null;

    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    public UserActionsReportParams Params { get { return (UserActionsReportParams)(base.ReportParams); } }

    private EFPReportDBxGridPage _MainPage;

    #endregion

    #region Запрос параметров

    protected override EFPReportParams CreateParams()
    {
      return new UserActionsReportParams(UI);
    }

    protected override bool QueryParams()
    {
      UserActionsReportParamsForm paramForm = new UserActionsReportParamsForm(UI);
      paramForm.efpPeriod.First.NValue = Params.FirstDate;
      paramForm.efpPeriod.Last.NValue = Params.LastDate;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
        paramForm.efpUser.DocId = Params.UserId;
      paramForm.efpOneType.Checked = !String.IsNullOrEmpty(Params.SingleDocTypeName);
      try
      {
        paramForm.efpDocType.DocTypeName = Params.SingleDocTypeName;
      }
      catch { }

      if (EFPApp.ShowDialog(paramForm, true) != DialogResult.OK)
        return false;

      Params.FirstDate = paramForm.efpPeriod.First.NValue;
      Params.LastDate = paramForm.efpPeriod.Last.NValue;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
        Params.UserId = paramForm.efpUser.DocId;
      if (paramForm.efpOneType.Checked)
        Params.SingleDocTypeName = paramForm.efpDocType.DocTypeName;
      else
        Params.SingleDocTypeName = String.Empty;
      return true;
    }

    #endregion

    #region Построение отчета

    protected override void BuildReport()
    {
      using (new Splash(Res.Common_Phase_GetUserActionTable))
      {
        _MainPage.DataSource = UI.DocProvider.GetUserActionsTable(Params.FirstDate, Params.LastDate, Params.UserId, Params.SingleDocTypeName).DefaultView;
      }
    }

    #endregion

    #region Список действий (Основная страница)

    void MainPage_InitGrid(object sender, EventArgs args)
    {
      _MainPage.ControlProvider.Columns.AddInt("Id", true, "UserActionId", 5);
      _MainPage.ControlProvider.Columns.AddDateTime("StartTime", true, Res.Common_ColTitle_ActionStartTime);
      _MainPage.ControlProvider.Columns.AddDateTime("ActionTime", true, Res.Common_ColTitle_ActionTime);
      _MainPage.ControlProvider.Columns.AddInt("ApplyChangesCount", true, Res.Common_ColTitle_ApplyChangesCount, 4);
      _MainPage.ControlProvider.Columns.AddDateTime("ApplyChangesTime", true, Res.Common_ColTitle_ApplyChangesTime);
      _MainPage.ControlProvider.Columns.AddText("EditTime", false, Res.Common_ColTitle_EditTime, 10);
      _MainPage.ControlProvider.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
      {
        if (UI.DebugShowIds)
          _MainPage.ControlProvider.Columns.AddInt("UserId", true, "UserId", 5);
        _MainPage.ControlProvider.Columns.AddText("UserId.UserName", false, Res.Common_ColTitle_UserName, 15, 5);
      }
      if (UI.DocProvider.DocTypes.UseSessionId)
        _MainPage.ControlProvider.Columns.AddInt("SessionId", true, "SessionId", 5);
      _MainPage.ControlProvider.Columns.AddTextFill("ActionInfo", true, Res.Common_ColTitle_ActionInfo, 100, 20);
      _MainPage.ControlProvider.DisableOrdering();

      _MainPage.ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(MainGridHandler_GetRowAttributes);
      _MainPage.ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(MainGridHandler_GetCellAttributes);

      _MainPage.ControlProvider.ConfigSectionName = ConfigSectionName;

      _MainPage.ControlProvider.ReadOnly = true;
      _MainPage.ControlProvider.Control.ReadOnly = true;
      _MainPage.ControlProvider.CanMultiEdit = false;
      _MainPage.ControlProvider.CanView = true;
      _MainPage.ControlProvider.EditData += new EventHandler(MainGridHandler_EditData);

      _MainPage.ControlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(MainGridHandler_GetDocSel);

    }

    private static void MainGridHandler_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
    }

    private static void MainGridHandler_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      EFPDBxGridView ControlProvider = (EFPDBxGridView)sender;

      switch (args.ColumnName)
      {
        case "UserId.UserName":
          Int32 userId = DataTools.GetInt(args.DataRow, "UserId");
          if (userId != 0)
            args.Value = ControlProvider.UI.TextHandlers.GetTextValue(ControlProvider.UI.DocProvider.DocTypes.UsersTableName, userId);
          break;
        case "EditTime":
          DateTime? startTime = DataTools.GetNullableDateTime(args.DataRow, "StartTime");
          DateTime? applyChangesTime = DataTools.GetNullableDateTime(args.DataRow, "ApplyChangesTime");
          if (startTime.HasValue && applyChangesTime.HasValue)
            args.Value = applyChangesTime.Value - startTime.Value;
          break;
      }
    }

    private void MainGridHandler_EditData(object sender, EventArgs args)
    {
      // Открытие дополнительной закладки для редактирования
      EFPDBxGridView controlProvider = (EFPDBxGridView)sender;
      if (controlProvider.State != UIDataState.View)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_InknownEditMode);
        return;
      }
      DataRow row = controlProvider.CurrentDataRow;
      if (row == null)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedRow);
        return;
      }

      Int32 actionId = DataTools.GetInt(row, "Id");

      if (Pages.FindAndActivate(actionId.ToString()))
        return; // Уже была закладка


      EFPReportDBxGridPage actionPage = new EFPReportDBxGridPage(UI);
      actionPage.Title = actionId.ToString();
      actionPage.ToolTipText = DataTools.GetDateTime(row, "ActionTime").ToString() + Environment.NewLine +
        DataTools.GetString(row, "ActionInfo");
      actionPage.InitGrid += new EventHandler(ActionPage_InitGrid);

      actionPage.DataSource = UI.DocProvider.GetUserActionDocTable(actionId).DefaultView;

      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
      {
        Int32 userId = DataTools.GetInt(row, "UserId");
        string userName = UI.DocTypes[UI.DocProvider.DocTypes.UsersTableName].GetTextValue(userId);
        Int32 sessionId = 0;
        if (UI.DocProvider.DocTypes.UseSessionId)
          sessionId = DataTools.GetInt(row, "SessionId");
        actionPage.FilterInfo.Add(Res.UserActionReport_Name_UserFilter, userName + " (UserId=" + userId.ToString() + ")" +
          (sessionId == 0 ? String.Empty : (" (SessionId=" + sessionId.ToString() + ")")));
        if (EFPApp.ShowListImages)
          actionPage.FilterInfo.LastAdded.ImageKey = UI.GetUserImageKey(userId);
      }
      DateTime? actionTime = DataTools.GetNullableDateTime(row, "ActionTime");
      string actionInfo = DataTools.GetString(row, "ActionInfo");


      actionPage.FilterInfo.Add(Res.Common_ColTitle_ActionTime, actionTime.ToString());
      actionPage.FilterInfo.Add(Res.Common_ColTitle_ActionInfo, actionInfo);
      actionPage.ExtraPageKey = actionId.ToString();
      Pages.Add(actionPage);
    }

    private void MainGridHandler_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      try
      {
        //EFPDBxGridView ControlProvider = (EFPDBxGridView)sender;

        Splash spl = new Splash(Res.Common_Phase_DataLoad);
        try
        {
          spl.PercentMax = args.DataRows.Length;
          spl.AllowCancel = true;
          for (int i = 0; i < args.DataRows.Length; i++)
          {
            Int32 actionId = DataTools.GetInt(args.DataRows[i], "Id");
            DataTable table = UI.DocProvider.GetUserActionDocTable(actionId); // не оптимально
            foreach (DataRow row in table.Rows)
            {
              Int32 docTableId = DataTools.GetInt(row, "DocTableId");
              DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(docTableId);
              if (dt == null)
                continue;
              Int32 DocId = DataTools.GetInt(row, "DocId");
              args.DocSel.Add(dt.Name, DocId);
            }
            spl.IncPercent();
          }
        }
        finally
        {
          spl.Close();
        }
      }
      catch (UserCancelException)
      {
      }
    }

    #endregion

    #region Закладка для одного действия

    void ActionPage_InitGrid(object sender, EventArgs args)
    {
      EFPReportDBxGridPage actionPage = (EFPReportDBxGridPage)sender;

      actionPage.ControlProvider.Columns.AddImage("Image1");
      if (UI.DebugShowIds)
        actionPage.ControlProvider.Columns.AddInt("DocTableId", true, "DocTableId", 5);
      actionPage.ControlProvider.Columns.AddText("DocType", false, Res.Common_ColTitle_DocType, 10, 5);

      actionPage.ControlProvider.Columns.AddImage("Image2");
      if (UI.DebugShowIds)
        actionPage.ControlProvider.Columns.AddInt("Action", true, "Action", 5);
      actionPage.ControlProvider.Columns.AddText("ActionInfo", false, Res.Common_ColTitle_ActionInfo, 10, 5);

      actionPage.ControlProvider.Columns.AddInt("DocId", true, "DocId", 5);
      actionPage.ControlProvider.Columns.AddTextFill("DocText", false, Res.Common_ColTitle_DocText, 100, 30);
      actionPage.ControlProvider.Columns.AddInt("Id", true, "DocActionId", 5);

      actionPage.ControlProvider.DisableOrdering();
      actionPage.ControlProvider.ConfigSectionName = "OneActionView";

      actionPage.ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ActionGridHandler_GetRowAttributes);
      actionPage.ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ActionGridHandler_GetCellAttributes);
      actionPage.ControlProvider.ShowRowCountInTopLeftCell = true;


      actionPage.ControlProvider.CanMultiEdit = false;
      actionPage.ControlProvider.ReadOnly = true;
      actionPage.ControlProvider.CanView = true;
      actionPage.ControlProvider.EditData += new EventHandler(ActionGridHandler_EditData);

      EFPCommandItem ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
      ciShowDocInfo.MenuText = Res.Cmd_Menu_DocInfo;
      ciShowDocInfo.ShortCut = Keys.F12;
      ciShowDocInfo.ImageKey = "Information";
      ciShowDocInfo.Tag = actionPage.ControlProvider;
      ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
      actionPage.ControlProvider.CommandItems.Add(ciShowDocInfo);

      actionPage.ControlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(ActionPage_GetDocSel);
    }


    /// <summary>
    /// Буферизуем поиск вида документа для ускорения вычислений
    /// </summary>
    private DocTypeUI _CurrDocType;
    private DataRow _CurrRow;

    void ActionGridHandler_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      _CurrRow = args.DataRow;
      if (_CurrRow == null)
        return;
      Int32 docTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType docType = UI.DocProvider.DocTypes.FindByTableId(docTableId);
      if (docType == null)
        _CurrDocType = null;
      else
        _CurrDocType = UI.DocTypes[docType.Name];
    }

    void ActionGridHandler_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      UndoAction action;
      string imageKey;
      if (_CurrRow == null)
        return;
      switch (args.ColumnName)
      {
        case "Image1":
          if (_CurrDocType == null)
            imageKey = "No";
          else
          {
            if (String.IsNullOrEmpty(_CurrDocType.ImageKey))
              imageKey = "Item";
            else
              imageKey = _CurrDocType.ImageKey;
          }
          args.Value = EFPApp.MainImages.Images[imageKey];
          break;
        case "DocType":
          if (_CurrDocType == null)
            args.Value = "?? " + DataTools.GetInt(_CurrRow, "DocTableId").ToString();
          else
            args.Value = _CurrDocType.DocType.SingularTitle;
          break;
        case "Image2":
          action = (UndoAction)(DataTools.GetInt(_CurrRow, "Action"));
          imageKey = DBUI.GetUndoActionImageKey(action);
          args.Value = EFPApp.MainImages.Images[imageKey];
          break;
        case "ActionInfo":
          action = (UndoAction)(DataTools.GetInt(_CurrRow, "Action"));
          args.Value = DBxDocProvider.GetUndoActionName((UndoAction)action);
          break;
        case "DocText":
          Int32 docId = DataTools.GetInt(_CurrRow, "DocId");
          if (_CurrDocType == null)
            args.Value = "DocId=" + docId.ToString();
          else
            args.Value = _CurrDocType.GetTextValue(docId);
          break;
        default:
          return;
      }
    }

    void ActionGridHandler_EditData(object sender, EventArgs args)
    {
      EFPDBxGridView controlProvider = (EFPDBxGridView)sender;
      if (!controlProvider.CheckSingleRow())
        return;
      DataRow row = controlProvider.CurrentDataRow;
      Int32 docTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(docTableId);
      if (dt == null)
      {
        EFPApp.MessageBox(String.Format(Res.UserActionReport_Err_UnknownTableId, docTableId),
        Res.UserActionReport_ErrTitle_UnknownDocType, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      Int32 docId = DataTools.GetInt(_CurrRow, "DocId");
      if (docId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }

      //DocTypeUI dt2 = UI.DocTypes[dt.Name];

      //dt2.PerformEditing(DocId, true);
      // Просмотр истории
      int version = DataTools.GetInt(row, "Version");

      DBxDocSet docSet = new DBxDocSet(UI.DocProvider);
      docSet[dt.Name].ViewVersion(docId, version);
      DocumentEditor de = new DocumentEditor(UI, docSet);
      de.Run();

    }

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPDBxGridView controlProvider = (EFPDBxGridView)(ci.Tag);

      DataRow row = controlProvider.CurrentDataRow;
      if (row == null)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedRow);
        return;
      }
      Int32 docTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(docTableId);
      if (dt == null)
      {
        EFPApp.MessageBox(String.Format(Res.UserActionReport_Err_UnknownTableId, docTableId),
        Res.UserActionReport_ErrTitle_UnknownDocType, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      Int32 docId = DataTools.GetInt(_CurrRow, "DocId");
      if (docId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }

      DocTypeUI dt2 = UI.DocTypes[dt.Name];
      dt2.ShowDocInfo(docId);
    }

    void ActionPage_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      for (int i = 0; i < args.DataRows.Length; i++)
      {
        Int32 docTableId = DataTools.GetInt(args.DataRows[i], "DocTableId");
        DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(docTableId);
        if (dt == null)
          continue;
        Int32 docId = DataTools.GetInt(args.DataRows[i], "DocId");
        args.DocSel.Add(dt.Name, docId);
      }
    }

    #endregion
  }
}
