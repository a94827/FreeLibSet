// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.FIAS;
using System.Data;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Табличный просмотр справочника адресных объектов, домов или помещений
  /// </summary>
  internal class EFPFiasListDataGridView : EFPDataGridView
  {
    #region Конструкторы

    public EFPFiasListDataGridView(EFPBaseProvider baseProvider, DataGridView control, FiasUI ui, FiasTableType tableType, bool isHistView)
      : base(baseProvider, control)
    {
      Init(ui, tableType, isHistView);
    }

    public EFPFiasListDataGridView(EFPControlWithToolBar<DataGridView> controlWithToolBar, FiasUI ui, FiasTableType tableType, bool isHistView)
      : base(controlWithToolBar)
    {
      Init(ui, tableType, isHistView);
    }

    private void Init(FiasUI ui, FiasTableType tableType, bool isHistView)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
      _Handler = new FiasHandler(ui.Source);

      _TableType = tableType;

      posStartDate = -1;
      posEndDate = -1;

      Control.AutoGenerateColumns = false;

      Columns.AddImage("Image");

      switch (tableType)
      {
        case FiasTableType.AddrOb:
          Columns.AddTextFill("OFFNAME", true, "Наименование", 50, 25);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("SOCRNAME", false, "Сокращение", 10);

          Columns.AddTextFill("PARENTNAME", false, "Родительский объект", 50, 40);
          Columns.LastAdded.Grayed = true;

          if (ui.ShowGuids)
          {
            Columns.AddText("AOGUID", true, "AOGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("AOID", true, "AOID (неустойчивый идентификатор записи)", 36);
            Columns.LastAdded.CanIncSearch = true;
            Columns.LastAdded.Grayed = true;
          }
          if (_Handler.Source.DBSettings.UseHistory)
          {
            Columns.AddBool("Actual", true, "Actual");
            Columns.AddBool("Live", true, "Live");
          }
          Columns.AddText("POSTALCODE", true, "Почтовый индекс", 6, 6); 
          Columns.LastAdded.TextAlign = HorizontalAlignment.Center;

          if (_Handler.Source.DBSettings.UseOKATO)
            Columns.AddText("OKATO", true, "ОКАТО", 11, 11);
          if (_Handler.Source.DBSettings.UseOKTMO)
            Columns.AddText("OKTMO", true, "ОКТМО", 11, 11);
          if (_Handler.Source.DBSettings.UseIFNS)
          {
            Columns.AddText("IFNSFL", true, "ИФНС ФЛ", 4, 4);
            Columns.AddText("IFNSUL", true, "ИФНС ЮЛ", 4, 4);
          }

          FrozenColumns = 3;
          break;

        case FiasTableType.House:
          Columns.AddTextFill("HOUSETEXT", false, "Текстовое представление", 100, 20);

          Columns.AddText("HOUSENUM", true, "Дом", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("BUILDNUM", true, "Корпус", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("STRUCNUM", true, "Строение", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          if (ui.ShowGuids)
          {
            Columns.AddText("HOUSEGUID", true, "HOUSEGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("HOUSEID", true, "HOUSEID (неустойчивый идентификатор записи)", 36);
            Columns.LastAdded.CanIncSearch = true;
            Columns.LastAdded.Grayed = true;
          }
          Columns.AddText("POSTALCODE", true, "Почтовый индекс", 6, 6);
          Columns.LastAdded.TextAlign = HorizontalAlignment.Center;

          if (_Handler.Source.DBSettings.UseOKATO)
            Columns.AddText("OKATO", true, "ОКАТО", 11, 11);
          if (_Handler.Source.DBSettings.UseOKTMO)
            Columns.AddText("OKTMO", true, "ОКТМО", 11, 11);
          if (_Handler.Source.DBSettings.UseIFNS)
          {
            Columns.AddText("IFNSFL", true, "ИФНС ФЛ", 4, 4);
            Columns.AddText("IFNSUL", true, "ИФНС ЮЛ", 4, 4);
          }

          FrozenColumns = 2;
          break;

        case FiasTableType.Room:
          Columns.AddTextFill("ROOMTEXT", false, "Текстовое представление", 100, 20);
          Columns.AddText("FLATNUMBER", true, "Квартира", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("ROOMNUMBER", true, "Комната", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          if (ui.ShowGuids)
          {
            Columns.AddText("ROOMGUID", true, "ROOMGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("ROOMID", true, "ROOMID (неустойчивый идентификатор записи)", 36);
            Columns.LastAdded.CanIncSearch = true;
            Columns.LastAdded.Grayed = true;
          }
          if (_Handler.Source.DBSettings.UseHistory)
          {
            Columns.AddBool("Live", true, "Live");
          }

          FrozenColumns = 2;
          break;

        default:
          throw new ArgumentException("Неизвестная таблица " + tableType.ToString(), "tableType");
      }

      if (ui.ShowDates)
      {
        if (ui.InternalSettings.UseOADates)
        {
          Columns.AddDate("STARTDATE", false, "Начало действия");
          Columns.AddDate("ENDDATE", false, "Окончание действия");
        }
        else
        {
          // Обычные столбцы

          Columns.AddDate("STARTDATE", true, "Начало действия");
          Columns.AddDate("ENDDATE", true, "Окончание действия");
          return;
        }
      }

      _UseNextPrev = false;
      if (isHistView && ui.ShowGuids)
      {
        switch (tableType)
        {
          case FiasTableType.AddrOb:
          case FiasTableType.Room:
            _UseNextPrev = true;
            break;
        }
      }


      if (UseNextPrev)
      {
        // 26.02.2021 - Просмотр идентификаторов NEXTID и PREVID
        Columns.AddText("PREVID", true, "PREVID", 36);
        Columns.LastAdded.CanIncSearch = true;
        Columns.LastAdded.Grayed = true;

        Columns.AddText("NEXTID", true, "NEXTID", 36);
        Columns.LastAdded.CanIncSearch = true;
        Columns.LastAdded.Grayed = true;
      }

      DisableOrdering();
      ShowRowCountInTopLeftCellToolTipText = true;

      Control.ReadOnly = true;
      ReadOnly = true;

      if ((!isHistView) && ui.DBSettings.UseHistory /* 25.01.2022 */)
      {
        CanView = true;
        EditData += new EventHandler(EFPFiasListDataGridView_EditData);
      }
      else
        CanView = false;

      if (UseNextPrev)
      {
        ciGotoPrev = new EFPCommandItem("View", "GoToPrev");
        ciGotoPrev.MenuText = "Предыдущая историческая запись";
        ciGotoPrev.ImageKey = "ArrowLeft";
        ciGotoPrev.Click += new EventHandler(ciGotoPrev_Click);
        ciGotoPrev.GroupBegin = true;
        CommandItems.Add(ciGotoPrev);

        ciGotoNext = new EFPCommandItem("View", "GoToNext");
        ciGotoNext.MenuText = "Следующая историческая запись";
        ciGotoNext.ImageKey = "ArrowRight";
        ciGotoNext.Click += new EventHandler(ciGotoNext_Click);
        ciGotoNext.GroupEnd = true;
        CommandItems.Add(ciGotoNext);

        UseIdle = true;
      }
    }

    protected override void OnCreated()
    {
      base.OnCreated();

      InitRecIdRows();

      if (_UI.InternalSettings.UseOADates)
      {
        posStartDate = SourceAsDataTable.Columns.IndexOf("dStartDate");
        posEndDate = SourceAsDataTable.Columns.IndexOf("dEndDate");
      }
      else
      {
        posStartDate = SourceAsDataTable.Columns.IndexOf("STARTDATE");
        posEndDate = SourceAsDataTable.Columns.IndexOf("ENDDATE");
      }
    }

    #endregion

    #region Свойства

    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    private FiasHandler _Handler;

    public FiasTableType TableType { get { return _TableType; } }
    private FiasTableType _TableType;

    protected override string DefaultDisplayName
    {
      get
      {
        return "Таблица \"" + FiasEnumNames.ToString(_TableType) + "\"";
      }
    }

    #endregion

    #region Оформление просмотра

    private Dictionary<Guid, string> _ParentNames;

    private int posStartDate, posEndDate;

    /// <summary>
    /// Возвращает период действия строки данных STARTDATE-ENDDATE
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private DateRange GetDateRange(DataRow row)
    {
      if (posStartDate >= 0)
      {
        if (_UI.InternalSettings.UseOADates)
        {
          int v1 = DataTools.GetInt(row[posStartDate]);
          int v2 = DataTools.GetInt(row[posEndDate]);
          DateTime dt1 = (v1 == 0) ? DateRange.Whole.FirstDate : DateTime.FromOADate(v1);
          DateTime dt2 = (v2 == 0) ? DateRange.Whole.LastDate : DateTime.FromOADate(v2);
          return new DateRange(dt1, dt2);
        }
        else
        {
          DateTime? dt1 = DataTools.GetNullableDateTime(row[posStartDate]);
          DateTime? dt2 = DataTools.GetNullableDateTime(row[posEndDate]);
          return new DateRange(dt1, dt2);
        }
      }
      else
        return DateRange.Whole;
    }

    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      if (args.DataRow != null)
      {
        switch (_TableType)
        {
          case FiasTableType.AddrOb:
            if (_UI.DBSettings.UseHistory)
            {
              bool Actual = DataTools.GetBool(args.DataRow, "Actual");
              bool Live = DataTools.GetBool(args.DataRow, "Live");
              if (!(Actual && Live))
                args.Grayed = true;
            }
            break;
          case FiasTableType.Room:
            if (_UI.DBSettings.UseHistory)
            {
              bool Live = DataTools.GetBool(args.DataRow, "Live");
              if (!Live)
                args.Grayed = true;
            }
            break;
          default: // 26.02.2021
            if (!GetDateRange(args.DataRow).Contains(DateTime.Today))
              args.Grayed = true;
            break;
        }
      }

      base.OnGetRowAttributes(args);
    }

    protected override void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      string errorText;

      if (args.DataRow != null)
      {
        switch (args.ColumnName)
        {
          case "Image":
            bool Actual;
            if (_UI.DBSettings.UseHistory)
            {
              if (_TableType == FiasTableType.AddrOb)
                Actual = DataTools.GetBool(args.DataRow, "Actual");
              else
                Actual = GetDateRange(args.DataRow).Contains(DateTime.Today); // 26.02.2021
            }
            else
              Actual = true;
            args.Value = EFPApp.MainImages.Images[GetImageKey(Actual ? FiasActuality.Actual : FiasActuality.Historical)];
            break;

          #region Вызовы IsValidName()

          // 03.03.2021
          // Вызываем IsValidName() в основном, чтобы протестировать эту функцию

          case "OFFNAME":
            FiasLevel level = (FiasLevel)(DataTools.GetInt(args.DataRow, "AOLEVEL"));
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, "OFFNAME"), level, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;
          case "HOUSENUM":
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, args.ColumnName),
              FiasLevel.House, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;
          case "BUILDNUM":
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, args.ColumnName),
              FiasLevel.Building, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;
          case "STRUCNUM":
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, args.ColumnName),
              FiasLevel.Structure, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;
          case "FLATNUMBER":
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, args.ColumnName),
              FiasLevel.Flat, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;
          case "ROOMNUMBER":
            if (!FiasTools.IsValidName(DataTools.GetString(args.DataRow, args.ColumnName),
              FiasLevel.Room, out errorText))
            {
              args.ColorType = EFPDataGridViewColorType.Error;
              args.ToolTipText = errorText;
            }
            break;

          #endregion

          case "SOCRNAME":
            Int32 aoTypeId = DataTools.GetInt(args.DataRow, "AOTypeId");
            args.Value = _Handler.AOTypes.GetAOType(aoTypeId, FiasAOTypeMode.Full);
            break;
          case "PARENTNAME":
            Guid ParentGuid = DataTools.GetGuid(args.DataRow, "PARENTGUID");
            if (_ParentNames == null)
              _ParentNames = new Dictionary<Guid, string>();
            string parentname;
            if (!_ParentNames.TryGetValue(ParentGuid, out parentname))
            {
              if (ParentGuid == Guid.Empty)
                parentname = FiasTools.RF;
              else
              {
                FiasAddress addr = new FiasAddress();
                addr.AOGuid = ParentGuid;
                _Handler.FillAddress(addr);
                FiasLevel lvl = addr.GuidBottomLevel;
                if (lvl == FiasLevel.Unknown)
                  parentname = "???";
                else 
                  //parentname = addr[lvl];
                  parentname = _Handler.Format(addr, lvl); // 30.03.2022
              }

              _ParentNames.Add(ParentGuid, parentname);
            }
            args.Value = parentname;
            break;

          case "HOUSETEXT":
            args.Value = FiasCachedPageHouse.GetText(args.DataRow);
            break;

          case "ROOMTEXT":
            args.Value = FiasCachedPageRoom.GetText(args.DataRow);
            break;

          case "STARTDATE":
            args.Value = GetOADate(args.DataRow, "dStartDate");
            break;
          case "ENDDATE":
            args.Value = GetOADate(args.DataRow, "dEndDate");
            break;

          case "NEXTID":
          case "PREVID":
            if (_RecIdRows != null)
            {
              if (!args.DataRow.IsNull(args.ColumnName))
              {
                Guid RefGuid = DataTools.GetGuid(args.DataRow, args.ColumnName);
                if (!_RecIdRows.ContainsKey(RefGuid))
                {
                  args.ColorType = EFPDataGridViewColorType.Error;
                  args.ToolTipText = "Ссылка на историческую запись не найдена";
                }
              }
            }
            break;
          case "POSTALCODE":
          case "IFNSFL":
          case "IFNSUL":
            if (DataTools.GetInt(args.DataRow, args.ColumnName) == 0)
              args.Value = null; // поле является числовым
            break;
          case "OKATO":
          case "OKTMO":
            if (DataTools.GetInt64(args.DataRow, args.ColumnName) == 0L)
              args.Value = null; // поле является числовым
            break;
        }
      }

      base.OnGetCellAttributes(args);
    }

    private static object GetOADate(DataRow row, string colName)
    {
      int v = DataTools.GetInt(row, colName);
      if (v == 0)
        return null;
      return DateTime.FromOADate(v);
    }

    public static string GetImageKey(FiasActuality actuality)
    {
      switch (actuality)
      {
        case FiasActuality.Unknown: return "UnknownState";
        case FiasActuality.Actual: return "Item";
        case FiasActuality.Historical: return "Delete";
        default: return "Error";
      }
    }

    #endregion

    #region Просмотр истории изменения адресного объекта

    void EFPFiasListDataGridView_EditData(object sender, EventArgs args)
    {
      if (!CheckSingleRow())
        return;

      string colName;
      switch (_TableType)
      {
        case FiasTableType.AddrOb: colName = "AOGUID"; break;
        case FiasTableType.House: colName = "HOUSEGUID"; break;
        case FiasTableType.Room: colName = "ROOMGUID"; break;
        default:
          throw new BugException();
      }

      Guid g = DataTools.GetGuid(CurrentDataRow, colName);
      _UI.ShowHistory(_TableType, g);
    }

    #endregion

    #region Переход к следующей/предыдущей исторической записи

    /// <summary>
    /// true, если есть команды перехода между историческими записями
    /// </summary>
    internal bool UseNextPrev { get { return _UseNextPrev; } }
    private bool _UseNextPrev;

    /// <summary>
    /// Используется для раскраски полей NEXTID и PREVID и для перехода между историческими записям.
    /// Ключ - неустойчивый идентификатор записи.
    /// Значение - строка таблицы
    /// </summary>
    private Dictionary<Guid, DataRow> _RecIdRows;

    private void InitRecIdRows()
    {
      if (UseNextPrev)
      {
        string columnName;
        switch (TableType)
        {
          case FiasTableType.AddrOb: columnName = "AOID"; break;
          case FiasTableType.House: columnName = "HOUSEID"; break;
          case FiasTableType.Room: columnName = "ROOMID"; break;
          default:
            throw new BugException();
        }

        _RecIdRows = new Dictionary<Guid, DataRow>(SourceAsDataTable.Rows.Count);
        foreach (DataRow row in SourceAsDataTable.Rows)
        {
          Guid g = DataTools.GetGuid(row, columnName);
          _RecIdRows[g] = row;
        }
      }
    }

    EFPCommandItem ciGotoPrev, ciGotoNext;

    public override void HandleIdle()
    {
      base.HandleIdle();

      if (ciGotoNext != null)
      {
        DataRow row = this.CurrentDataRow;
        if (row == null)
        {
          ciGotoPrev.Enabled = false;
          ciGotoNext.Enabled = false;
        }
        else
        {
          ciGotoPrev.Enabled = !row.IsNull("PREVID");
          ciGotoNext.Enabled = !row.IsNull("NEXTID");
        }
      }
    }

    void ciGotoPrev_Click(object sender, EventArgs args)
    {
      GotoNextPrev("PREVID");
    }

    void ciGotoNext_Click(object sender, EventArgs args)
    {
      GotoNextPrev("NEXTID");
    }

    private void GotoNextPrev(string columnName)
    {
#if DEBUG
      if (_RecIdRows == null)
        throw new BugException("RecIdRows==null");
#endif
      if (!CheckSingleRow())
        return;
      if (CurrentDataRow.IsNull(columnName))
      {
        EFPApp.ShowTempMessage("Цепочка перехода закончена");
        return;
      }
      Guid g = DataTools.GetGuid(CurrentDataRow, columnName);
      DataRow row;
      if (_RecIdRows.TryGetValue(g, out row))
        CurrentDataRow = row;
      else
        EFPApp.ShowTempMessage("Не найдена историческая запись для перехода");
    }

    #endregion
  }
}
