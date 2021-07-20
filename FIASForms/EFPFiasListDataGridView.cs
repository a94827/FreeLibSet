using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.FIAS;
using System.Data;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.FIAS
{
  /// <summary>
  /// ��������� �������� ����������� �������� ��������, ����� ��� ���������
  /// </summary>
  internal class EFPFiasListDataGridView : EFPDataGridView
  {
    #region ������������

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
          Columns.AddTextFill("OFFNAME", true, "������������", 50, 25);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("SOCRNAME", false, "����������", 10);

          Columns.AddTextFill("PARENTNAME", false, "������������ ������", 50, 40);
          Columns.LastAdded.Grayed = true;

          if (ui.ShowGuidsInTables)
          {
            Columns.AddText("AOGUID", true, "AOGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("AOID", true, "AOID (������������ ������������� ������)", 36);
            Columns.LastAdded.CanIncSearch = true;
            Columns.LastAdded.Grayed = true;
          }
          if (_Handler.Source.DBSettings.UseHistory)
          {
            Columns.AddBool("Actual", true, "Actual");
            Columns.AddBool("Live", true, "Live");
          }
          FrozenColumns = 3;
          break;

        case FiasTableType.House:
          Columns.AddTextFill("HOUSETEXT", false, "��������� �������������", 100, 20);

          Columns.AddText("HOUSENUM", true, "���", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("BUILDNUM", true, "������", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("STRUCNUM", true, "��������", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          if (ui.ShowGuidsInTables)
          {
            Columns.AddText("HOUSEGUID", true, "HOUSEGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("HOUSEID", true, "HOUSEID (������������ ������������� ������)", 36);
            Columns.LastAdded.CanIncSearch = true;
            Columns.LastAdded.Grayed = true;
          }
          FrozenColumns = 2;
          break;

        case FiasTableType.Room:
          Columns.AddTextFill("ROOMTEXT", false, "��������� �������������", 100, 20);
          Columns.AddText("FLATNUMBER", true, "��������", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          Columns.AddText("ROOMNUMBER", true, "�������", 15, 10);
          Columns.LastAdded.CanIncSearch = true;
          if (ui.ShowGuidsInTables)
          {
            Columns.AddText("ROOMGUID", true, "ROOMGUID", 36);
            Columns.LastAdded.CanIncSearch = true;

            Columns.AddText("ROOMID", true, "ROOMID (������������ ������������� ������)", 36);
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
          throw new ArgumentException("����������� ������� " + tableType.ToString(), "tableType");
      }

      if (ui.ShowDates)
      {
        if (ui.InternalSettings.UseOADates)
        {
          Columns.AddDate("STARTDATE", false, "������ ��������");
          Columns.AddDate("ENDDATE", false, "��������� ��������");
        }
        else
        {
          // ������� �������

          Columns.AddDate("STARTDATE", true, "������ ��������");
          Columns.AddDate("ENDDATE", true, "��������� ��������");
          return;
        }
      }

      _UseNextPrev = false;
      if (isHistView && ui.ShowGuidsInTables)
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
        // 26.02.2021 - �������� ��������������� NEXTID � PREVID
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
      CanView = !isHistView;
      if (!isHistView)
        EditData += new EventHandler(EFPFiasListDataGridView_EditData);

      if (UseNextPrev)
      {
        ciGotoPrev = new EFPCommandItem("View", "GoToPrev");
        ciGotoPrev.MenuText = "���������� ������������ ������";
        ciGotoPrev.ImageKey = "ArrowLeft";
        ciGotoPrev.Click += new EventHandler(ciGotoPrev_Click);
        ciGotoPrev.GroupBegin = true;
        CommandItems.Add(ciGotoPrev);

        ciGotoNext = new EFPCommandItem("View", "GoToNext");
        ciGotoNext.MenuText = "��������� ������������ ������";
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

    #region ��������

    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    private FiasHandler _Handler;

    public FiasTableType TableType { get { return _TableType; } }
    private FiasTableType _TableType;

    protected override string DefaultDisplayName
    {
      get
      {
        return "������� \"" + FiasEnumNames.ToString(_TableType) + "\"";
      }
    }

    #endregion

    #region ���������� ���������

    private Dictionary<Guid, string> _ParentNames;

    private int posStartDate, posEndDate;

    /// <summary>
    /// ���������� ������ �������� ������ ������ STARTDATE-ENDDATE
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

          #region ������ IsValidName

            // 03.03.2021
            // �������� IsValidName() � ��������, ����� �������������� ��� �������

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
                  parentname = addr[lvl].ToString();
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
                  args.ToolTipText = "������ �� ������������ ������ �� �������";
                }
              }
            }
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

    #region �������� ������� ��������� ��������� �������

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

    #region ������� � ���������/���������� ������������ ������

    /// <summary>
    /// true, ���� ���� ������� �������� ����� ������������� ��������
    /// </summary>
    internal bool UseNextPrev { get { return _UseNextPrev; } }
    private bool _UseNextPrev;

    /// <summary>
    /// ������������ ��� ��������� ����� NEXTID � PREVID � ��� �������� ����� ������������� �������.
    /// ���� - ������������ ������������� ������.
    /// �������� - ������ �������
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
        EFPApp.ShowTempMessage("������� �������� ���������");
        return;
      }
      Guid g = DataTools.GetGuid(CurrentDataRow, columnName);
      DataRow row;
      if (_RecIdRows.TryGetValue(g, out row))
        CurrentDataRow = row;
      else
        EFPApp.ShowTempMessage("�� ������� ������������ ������ ��� ��������");
    }

    #endregion
  }
}
