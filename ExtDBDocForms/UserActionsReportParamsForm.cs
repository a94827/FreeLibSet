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

      Icon = EFPApp.MainImageIcon("UserActions");

      efpForm = new EFPFormProvider(this);

      if (ui.DocProvider.DocTypes.UseUsers)
      {
        efpUser = new EFPDocComboBox(efpForm, cbUser, ui.DocTypes[ui.DocProvider.DocTypes.UsersTableName]);
        efpUser.ToolTipText = "Пользователь, действия которого просматриваются";
        efpUser.CanBeEmpty = true;
        efpUser.EmptyText = "[ Все пользователи ]";
        // TODO: if (!AccDepClientExec.UserIsAdministrator)
        //  efpUser.Enabled = false;
      }
      else // 21.05.2019
      {
        EFPUserSelComboBox efpDummy = new EFPUserSelComboBox(efpForm, cbUser);
        efpDummy.Visible = false;
      }

      efpPeriod = new EFPDateRangeBox(efpForm, edPeriod);
      efpPeriod.FirstDate.ToolTipText = "Начальная дата";
      efpPeriod.FirstDate.CanBeEmpty = true;
      efpPeriod.LastDate.ToolTipText = "Конечная дата";
      efpPeriod.LastDate.CanBeEmpty = true;

      btnLastDay.Image = EFPApp.MainImages.Images["ArrowUpThenLeft"];
      btnLastDay.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpLastDay = new EFPButton(efpForm, btnLastDay);
      efpLastDay.DisplayName = "Найти последнюю дату";
      if (ui.DocProvider.DocTypes.UseUsers)
      {
        efpLastDay.ToolTipText = "Устанавливает период на последний день, в который для выбранного пользователя есть действия." + Environment.NewLine +
          "Поле \"Один тип документов не учитывается\"";
        efpLastDay.EnabledEx = new DepExpr1<bool, int>(efpUser.DocIdEx, CalcLastDayEnabled);
        efpLastDay.Click += new EventHandler(efpLastDay_Click);
      }
      else
        efpLastDay.Visible = false; // 21.05.2019

      efpOneType = new EFPCheckBox(efpForm, cbOneType);
      efpOneType.ToolTipText = "Можно выбрать один справочник или журнал, чтобы уменьшить размер таблицы." + Environment.NewLine +
        "Если поле не заполнено, то будут показаны действия со всеми документами";

      efpDocType = new EFPDocTypeComboBox(efpForm, cbDocType, ui, null);
      efpDocType.ToolTipText = "Выбранный справочник или журнал";
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
        efpUser.SetFocus("Пользователь не выбран");
        return;
      }

      DateTime? dt = efpUser.UI.DocProvider.GetUserActionsLastTime(efpUser.DocId);
      if (dt.HasValue)
      {
        efpPeriod.FirstDate.NValue = dt.Value.Date;
        efpPeriod.LastDate.NValue = dt.Value.Date;
      }
      else
        efpUser.SetFocus("Для пользователя нет действий");

    }

    #endregion
  }

  /// <summary>
  /// Параметры отчета для просмотра действий пользователя.
  /// Заполненные параметры должны быть переданы в метод DBUI.ShowUserActions()
  /// </summary>
  public class UserActionsReportParams : EFPReportParams
  {
    #region Конструктор

    /// <summary>
    /// Создает параметры со значениями по умолчанию:
    /// - период (FirstDate:LastDate) равен одному дню - текущей дате;
    /// - пользователь UserId=DBxDocProvider.UserId;
    /// - SingleDocTypeName равен пустой строке
    /// </summary>
    /// <param name="ui">Интерфейс для доступа к документам</param>
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
    private DBUI _UI;

    #endregion

    #region Поля

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
    /// Если DBxDocTypes.UseUsers=false, свойство игнорируется.
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
          FilterInfo.Add("Пользователь", "Все пользователи");
        else
        {
          FilterInfo.Add("Пользователь", UI.GetUserName(UserId));
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
      Title = "Просмотр действий пользователя";
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
      _MainPage.Title = "Действия";
      _MainPage.ToolTipText = "Список действий";
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
      UserActionsReportParamsForm ParamForm = new UserActionsReportParamsForm(UI);
      ParamForm.efpPeriod.FirstDate.NValue = Params.FirstDate;
      ParamForm.efpPeriod.LastDate.NValue = Params.LastDate;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
        ParamForm.efpUser.DocId = Params.UserId;
      ParamForm.efpOneType.Checked = !String.IsNullOrEmpty(Params.SingleDocTypeName);
      try
      {
        ParamForm.efpDocType.DocTypeName = Params.SingleDocTypeName;
      }
      catch { }

      if (EFPApp.ShowDialog(ParamForm, true) != DialogResult.OK)
        return false;

      Params.FirstDate = ParamForm.efpPeriod.FirstDate.NValue;
      Params.LastDate = ParamForm.efpPeriod.LastDate.NValue;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
        Params.UserId = ParamForm.efpUser.DocId;
      if (ParamForm.efpOneType.Checked)
        Params.SingleDocTypeName = ParamForm.efpDocType.DocTypeName;
      else
        Params.SingleDocTypeName = String.Empty;
      return true;
    }

    #endregion

    #region Построение отчета

    protected override void BuildReport()
    {
      using (new Splash("Получение списка действий"))
      {
        _MainPage.DataSource = UI.DocProvider.GetUserActionsTable(Params.FirstDate, Params.LastDate, Params.UserId, Params.SingleDocTypeName).DefaultView;
      }
    }

    #endregion

    #region Список действий (Основная страница)

    void MainPage_InitGrid(object sender, EventArgs args)
    {
      _MainPage.ControlProvider.Columns.AddInt("Id", true, "UserActionId", 5);
      _MainPage.ControlProvider.Columns.AddDateTime("StartTime", true, "Время начала редактирования");
      _MainPage.ControlProvider.Columns.AddDateTime("ActionTime", true, "Время первого сохранения");
      _MainPage.ControlProvider.Columns.AddInt("ApplyChangesCount", true, "Кол-во сохранений", 4);
      _MainPage.ControlProvider.Columns.AddDateTime("ApplyChangesTime", true, "Время последнего сохранения");
      _MainPage.ControlProvider.Columns.AddText("EditTime", false, "Время редактирования", 10);
      _MainPage.ControlProvider.Columns.LastAdded.TextAlign = HorizontalAlignment.Right;
      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
      {
        if (UI.DebugShowIds)
          _MainPage.ControlProvider.Columns.AddInt("UserId", true, "UserId", 5);
        _MainPage.ControlProvider.Columns.AddText("UserId.UserName", false, "Пользователь", 15, 5);
      }
      if (UI.DocProvider.DocTypes.UseSessionId)
        _MainPage.ControlProvider.Columns.AddInt("SessionId", true, "SessionId", 5);
      _MainPage.ControlProvider.Columns.AddTextFill("ActionInfo", true, "Действие", 100, 20);
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
          Int32 UserId = DataTools.GetInt(args.DataRow, "UserId");
          if (UserId != 0)
            args.Value = ControlProvider.UI.TextHandlers.GetTextValue(ControlProvider.UI.DocProvider.DocTypes.UsersTableName, UserId);
          break;
        case "EditTime":
          DateTime? StartTime = DataTools.GetNullableDateTime(args.DataRow, "StartTime");
          DateTime? ApplyChangesTime = DataTools.GetNullableDateTime(args.DataRow, "ApplyChangesTime");
          if (StartTime.HasValue && ApplyChangesTime.HasValue)
            args.Value = ApplyChangesTime.Value - StartTime.Value;
          break;
      }
    }

    private void MainGridHandler_EditData(object sender, EventArgs args)
    {
      // Открытие дополнительной закладки для редактирования
      EFPDBxGridView ControlProvider = (EFPDBxGridView)sender;
      if (ControlProvider.State != EFPDataGridViewState.View)
      {
        EFPApp.ShowTempMessage("Неизвестный режим редактирования");
        return;
      }
      DataRow Row = ControlProvider.CurrentDataRow;
      if (Row == null)
      {
        EFPApp.ShowTempMessage("Строка не выбрана");
        return;
      }

      Int32 ActionId = DataTools.GetInt(Row, "Id");

      if (Pages.FindAndActivate(ActionId.ToString()))
        return; // Уже была закладка


      EFPReportDBxGridPage ActionPage = new EFPReportDBxGridPage(UI);
      ActionPage.Title = ActionId.ToString();
      ActionPage.ToolTipText = DataTools.GetDateTime(Row, "ActionTime").ToString() + Environment.NewLine +
        DataTools.GetString(Row, "ActionInfo");
      ActionPage.InitGrid += new EventHandler(ActionPage_InitGrid);

      ActionPage.DataSource = UI.DocProvider.GetUserActionDocTable(ActionId).DefaultView;

      if (UI.DocProvider.DocTypes.UseUsers) // 21.05.2019
      {
        Int32 UserId = DataTools.GetInt(Row, "UserId");
        string UserName = UI.DocTypes[UI.DocProvider.DocTypes.UsersTableName].GetTextValue(UserId);
        Int32 SessionId = 0;
        if (UI.DocProvider.DocTypes.UseSessionId)
          SessionId = DataTools.GetInt(Row, "SessionId");
        ActionPage.FilterInfo.Add("Пользователь", UserName + " (UserId=" + UserId.ToString() + ")" +
          (SessionId == 0 ? String.Empty : (" (SessionId=" + SessionId.ToString() + ")")));
        if (EFPApp.ShowListImages)
          ActionPage.FilterInfo.LastAdded.ImageKey = UI.GetUserImageKey(UserId);
      }
      DateTime? ActionTime = DataTools.GetNullableDateTime(Row, "ActionTime");
      string ActionInfo = DataTools.GetString(Row, "ActionInfo");


      ActionPage.FilterInfo.Add("Время", ActionTime.ToString());
      ActionPage.FilterInfo.Add("Действие", ActionInfo);
      ActionPage.ExtraPageKey = ActionId.ToString();
      Pages.Add(ActionPage);
    }

    private void MainGridHandler_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      try
      {
        //EFPDBxGridView ControlProvider = (EFPDBxGridView)sender;

        Splash spl = new Splash("Загрузка списка документов");
        try
        {
          spl.PercentMax = args.DataRows.Length;
          spl.AllowCancel = true;
          for (int i = 0; i < args.DataRows.Length; i++)
          {
            Int32 ActionId = DataTools.GetInt(args.DataRows[i], "Id");
            DataTable Table = UI.DocProvider.GetUserActionDocTable(ActionId); // не оптимально
            foreach (DataRow Row in Table.Rows)
            {
              Int32 DocTableId = DataTools.GetInt(Row, "DocTableId");
              DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(DocTableId);
              if (dt == null)
                continue;
              Int32 DocId = DataTools.GetInt(Row, "DocId");
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
      EFPReportDBxGridPage ActionPage = (EFPReportDBxGridPage)sender;

      ActionPage.ControlProvider.Columns.AddImage("Image1");
      if (UI.DebugShowIds)
        ActionPage.ControlProvider.Columns.AddInt("DocTableId", true, "DocTableId", 5);
      ActionPage.ControlProvider.Columns.AddText("ВидДокумента", false, "Вид документа", 10, 5);

      ActionPage.ControlProvider.Columns.AddImage("Image2");
      if (UI.DebugShowIds)
        ActionPage.ControlProvider.Columns.AddInt("Action", true, "Action", 5);
      ActionPage.ControlProvider.Columns.AddText("Действие", false, "Действие пользователя", 10, 5);

      ActionPage.ControlProvider.Columns.AddInt("DocId", true, "DocId", 5);
      ActionPage.ControlProvider.Columns.AddTextFill("Документ", false, "Документ", 100, 30);
      ActionPage.ControlProvider.Columns.AddInt("Id", true, "DocActionId", 5);

      ActionPage.ControlProvider.DisableOrdering();
      ActionPage.ControlProvider.ConfigSectionName = "ПросмотрОдногоДействия";

      ActionPage.ControlProvider.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(ActionGridHandler_GetRowAttributes);
      ActionPage.ControlProvider.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(ActionGridHandler_GetCellAttributes);
      ActionPage.ControlProvider.ShowRowCountInTopLeftCellToolTipText = true;


      ActionPage.ControlProvider.CanMultiEdit = false;
      ActionPage.ControlProvider.ReadOnly = true;
      ActionPage.ControlProvider.CanView = true;
      ActionPage.ControlProvider.EditData += new EventHandler(ActionGridHandler_EditData);

      EFPCommandItem ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
      ciShowDocInfo.MenuText = "Информация о документе";
      ciShowDocInfo.ToolTipText = "Информация о документе";
      ciShowDocInfo.ShortCut = Keys.F12;
      ciShowDocInfo.ImageKey = "Information";
      ciShowDocInfo.Tag = ActionPage.ControlProvider;
      ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
      ActionPage.ControlProvider.CommandItems.Add(ciShowDocInfo);

      ActionPage.ControlProvider.GetDocSel += new EFPDBxGridViewDocSelEventHandler(ActionPage_GetDocSel);
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
      int DocTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType DocType = UI.DocProvider.DocTypes.FindByTableId(DocTableId);
      if (DocType == null)
        _CurrDocType = null;
      else
        _CurrDocType = UI.DocTypes[DocType.Name];
    }

    void ActionGridHandler_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      UndoAction Action;
      string ImageKey;
      if (_CurrRow == null)
        return;
      switch (args.ColumnName)
      {
        case "Image1":
          if (_CurrDocType == null)
            ImageKey = "No";
          else
          {
            if (String.IsNullOrEmpty(_CurrDocType.ImageKey))
              ImageKey = "Item";
            else
              ImageKey = _CurrDocType.ImageKey;
          }
          args.Value = EFPApp.MainImages.Images[ImageKey];
          break;
        case "ВидДокумента":
          if (_CurrDocType == null)
            args.Value = "?? " + DataTools.GetInt(_CurrRow, "DocTableId").ToString();
          else
            args.Value = _CurrDocType.DocType.SingularTitle;
          break;
        case "Image2":
          Action = (UndoAction)(DataTools.GetInt(_CurrRow, "Action"));
          ImageKey = DBUI.GetUndoActionImageKey(Action);
          args.Value = EFPApp.MainImages.Images[ImageKey];
          break;
        case "Действие":
          Action = (UndoAction)(DataTools.GetInt(_CurrRow, "Action"));
          args.Value = DBUI.GetUndoActionName((UndoAction)Action);
          break;
        case "Документ":
          Int32 DocId = DataTools.GetInt(_CurrRow, "DocId");
          if (_CurrDocType == null)
            args.Value = "DocId=" + DocId.ToString();
          else
            args.Value = _CurrDocType.GetTextValue(DocId);
          break;
        default:
          return;
      }
    }

    void ActionGridHandler_EditData(object sender, EventArgs args)
    {
      EFPDBxGridView ControlProvider = (EFPDBxGridView)sender;
      if (!ControlProvider.CheckSingleRow())
        return;
      DataRow Row = ControlProvider.CurrentDataRow;
      int DocTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(DocTableId);
      if (dt == null)
      {
        EFPApp.MessageBox("Тип документа " + DocTableId.ToString() +
        " не зарегистрирован. Вероятно, модуль, который использовал такие документы, сейчас не подключен",
        "Неизвестный тип документа", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      Int32 DocId = DataTools.GetInt(_CurrRow, "DocId");
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Документ не выбран");
        return;
      }

      //DocTypeUI dt2 = UI.DocTypes[dt.Name];

      //dt2.PerformEditing(DocId, true);
      // Просмотр истории
      int Version = DataTools.GetInt(Row, "Version");

      DBxDocSet DocSet = new DBxDocSet(UI.DocProvider);
      DocSet[dt.Name].ViewVersion(DocId, Version);
      DocumentEditor de = new DocumentEditor(UI, DocSet);
      de.Run();

    }

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPDBxGridView ControlProvider = (EFPDBxGridView)(ci.Tag);

      DataRow Row = ControlProvider.CurrentDataRow;
      if (Row == null)
      {
        EFPApp.ShowTempMessage("Строка не выбрана");
        return;
      }
      int DocTableId = DataTools.GetInt(_CurrRow, "DocTableId");
      DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(DocTableId);
      if (dt == null)
      {
        EFPApp.MessageBox("Тип документа " + DocTableId.ToString() +
        " не зарегистрирован. Вероятно, модуль, который использовал такие документы, сейчас не подключен",
        "Неизвестный тип документа", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      Int32 DocId = DataTools.GetInt(_CurrRow, "DocId");
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Документ не выбран");
        return;
      }


      DocTypeUI dt2 = UI.DocTypes[dt.Name];
      dt2.ShowDocInfo(DocId);
    }

    void ActionPage_GetDocSel(object sender, EFPDBxGridViewDocSelEventArgs args)
    {
      for (int i = 0; i < args.DataRows.Length; i++)
      {
        Int32 DocTableId = DataTools.GetInt(args.DataRows[i], "DocTableId");
        DBxDocType dt = UI.DocProvider.DocTypes.FindByTableId(DocTableId);
        if (dt == null)
          continue;
        Int32 DocId = DataTools.GetInt(args.DataRows[i], "DocId");
        args.DocSel.Add(dt.Name, DocId);
      }
    }

    #endregion
  }
}