using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using FreeLibSet.FIAS;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;

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

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Провайдер табличного просмотра детальной информации по адресу
  /// </summary>
  public class EFPFiasAddressDetailGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Неинициализированный табличный просмотр</param>
    /// <param name="ui">Ссылка на описатель интерфейса ФИАС</param>
    public EFPFiasAddressDetailGridView(EFPBaseProvider baseProvider, DataGridView control, FiasUI ui)
      : base(baseProvider, control)
    {
      Init(ui);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolbar">Табличный просмотр и панели инструментов</param>
    /// <param name="ui">Ссылка на описатель интерфейса ФИАС</param>
    public EFPFiasAddressDetailGridView(IEFPControlWithToolBar<DataGridView> controlWithToolbar, FiasUI ui)
      : base(controlWithToolbar)
    {
      Init(ui);
    }

    private void Init(FiasUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
      _Handler = new FiasHandler(ui.Source);

      _Table = new DataTable();
      _Table.Columns.Add("Level", typeof(int));
      _Table.Columns.Add("LevelName", typeof(string));
      _Table.Columns.Add("Name", typeof(string));
      _Table.Columns.Add("Abbreviation", typeof(string));
      _Table.Columns.Add("Guid", typeof(Guid));
      _Table.Columns.Add("PostalCode", typeof(string));
      if (ui.DBSettings.UseOKATO)
        _Table.Columns.Add("OKATO", typeof(string));
      if (ui.DBSettings.UseOKTMO)
        _Table.Columns.Add("OKTMO", typeof(string));
      if (ui.DBSettings.UseIFNS)
      {
        _Table.Columns.Add("IFNSFL", typeof(string));
        _Table.Columns.Add("IFNSUL", typeof(string));
      }
      if (ui.ShowGuidsInTables)
        _Table.Columns.Add("RecId", typeof(Guid));
      if (ui.DBSettings.UseHistory)
      {
        _Table.Columns.Add("Actuality", typeof(int));
        _Table.Columns.Add("Live", typeof(bool));
      }
      if (ui.ShowDates)
      {
        _Table.Columns.Add("STARTDATE", typeof(DateTime));
        _Table.Columns.Add("ENDDATE", typeof(DateTime));
      }

      Control.Columns.Clear();
      Control.AutoGenerateColumns = false;
      Columns.AddImage("Image");
      Columns.AddText("LevelName", true, "Уровень", 10);
      Columns.AddText("Name", true, "Наименование", 20, 10);
      Columns.AddText("Abbreviation", true, "Сокращение", 10, 5);
      Columns.AddText("Guid", true, "GUID", 36, 36);
      Columns.AddText("PostalCode", true, "Почтовый индекс", 6, 6);
      if (ui.DBSettings.UseOKATO)
        Columns.AddText("OKATO", true, "ОКАТО", 11, 11);
      if (ui.DBSettings.UseOKTMO)
        Columns.AddText("OKTMO", true, "ОКТМО", 11, 11);
      if (ui.DBSettings.UseIFNS)
      {
        Columns.AddText("IFNSFL", true, "Код ИФНС ФЛ", 4, 4);
        Columns.AddText("IFNSUL", true, "Код ИФНС ЮЛ", 4, 4);
      }
      if (ui.ShowGuidsInTables)
        Columns.AddText("RecId", true, "RecId (неустойчивый идентификатор записи)", 36, 36);
      if (ui.DBSettings.UseHistory)
      {
        Columns.AddBool("Actual", false, "Актуальность");
        Columns.AddBool("Live", false, "Live");
      }
      if (ui.ShowDates)
      {
        Columns.AddDate("STARTDATE", true, "STARTDATE");
        Columns.AddDate("ENDDATE", true, "ENDDATE");
      }
      FrozenColumns = 2;

      Control.ReadOnly = true;
      this.ReadOnly = true;
      this.CanView = ui.DBSettings.UseHistory;
      this.EditData += new EventHandler(EFPFiasAddressDetailGridView_EditData);

      Control.DataSource = _Table;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Пользовательский интерфейс ФИАС
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    private FiasHandler _Handler;

    private DataTable _Table;

    /// <summary>
    /// Возвращает "Детальная информация об адресе"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return "Детальная информация об адресе"; }
    }

    #endregion

    #region Отображаемый адрес

    /// <summary>
    /// Основное свойство - Отображаемый адрес.
    /// </summary>
    public FiasAddress Address
    {
      get { return _Address; }
      set
      {
        _Address = value;
        _Table.Rows.Clear();

        if (value != null)
        {
          FiasAddress a2 = new FiasAddress();

          for (int i = 0; i < FiasTools.AllLevels.Length; i++)
          {
            a2.Clear();
            FiasLevel level = FiasTools.AllLevels[i];

            if (value.GetRecId(level) != Guid.Empty)             // TODO: !!!!
              a2.SetRecId(level, value.GetRecId(level));
            else if (value.GetGuid(level) != Guid.Empty)
              a2.SetGuid(level, value.GetGuid(level));
            else
              continue;

            _Handler.FillAddress(a2);

            switch (level)
            {
              case FiasLevel.House:
                if ((Address.GetName(FiasLevel.House) + Address.GetName(FiasLevel.Building) + Address.GetName(FiasLevel.Structure)).Length == 0)
                  // Если ничего не задано, выводим для здания
                  AddLevelRow(a2, FiasLevel.House);
                else
                {
                  if (Address.GetName(FiasLevel.House).Length > 0)
                    AddLevelRow(a2, FiasLevel.House);
                  if (Address.GetName(FiasLevel.Building).Length > 0)
                    AddLevelRow(a2, FiasLevel.Building);
                  if (Address.GetName(FiasLevel.Structure).Length > 0)
                    AddLevelRow(a2, FiasLevel.Structure);
                }
                break;
              case FiasLevel.Building:
              case FiasLevel.Structure:
                break;

              case FiasLevel.Flat:
                if ((Address.GetName(FiasLevel.Flat) + Address.GetName(FiasLevel.Room)).Length == 0)
                  // Если ничего не задано, выводим для квартиры
                  AddLevelRow(a2, FiasLevel.Flat);
                else
                {
                  if (Address.GetName(FiasLevel.Flat).Length > 0)
                    AddLevelRow(a2, FiasLevel.Flat);
                  if (Address.GetName(FiasLevel.Room).Length > 0)
                    AddLevelRow(a2, FiasLevel.Room);
                }
                break;
              case FiasLevel.Room:
                break;

              default: // адресный объект
                AddLevelRow(a2, level);
                break;
            }
          }
        }
      }
    }
    private FiasAddress _Address;

    private void AddLevelRow(FiasAddress a2, FiasLevel level)
    {
      DataRow row = _Table.NewRow();
      row["Level"] = (int)level;
      row["LevelName"] = "[" + FiasEnumNames.ToString(level, false) + "]";
      row["Name"] = a2.GetName(level);
      row["Abbreviation"] = a2.GetAOType(level);
      row["Guid"] = a2.GetGuid(level).ToString();
      if (_UI.ShowGuidsInTables)
        row["RecId"] = a2.GetRecId(level).ToString();

      row["PostalCode"] = a2.PostalCode;
      if (_UI.DBSettings.UseOKATO)
        row["OKATO"] = a2.OKATO;
      if (_UI.DBSettings.UseOKTMO)
        row["OKTMO"] = a2.OKTMO;
      if (_UI.DBSettings.UseIFNS)
      {
        row["IFNSFL"] = a2.IFNSFL;
        row["IFNSUL"] = a2.IFNSUL;
      }
      if (_UI.DBSettings.UseHistory)
      {
        row["Actuality"] = (int)(a2.Actuality);
        if (a2.Live.HasValue)
          row["Live"] = a2.Live.Value;
      }

      if (_UI.ShowDates)
      {
        DataTools.SetNullableDateTime(row, "STARTDATE", a2.StartDate);
        DataTools.SetNullableDateTime(row, "ENDDATE", a2.EndDate);
      }
      _Table.Rows.Add(row);
    }

    #endregion

    #region Оформление просмотра

    /// <summary>
    /// Получение атрибутов строки
    /// </summary>
    /// <param name="args"></param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      if (args.DataRow != null)
      {
        if (_UI.DBSettings.UseHistory)
        {
          FiasActuality actuality = (FiasActuality)DataTools.GetInt(args.DataRow, "Actuality");
          object xLive = args.DataRow["Live"];
          bool live = true;
          if (xLive is bool)
            live = (bool)xLive;
          if (actuality != FiasActuality.Actual || (!live))
            args.Grayed = true;
        }
      }

      base.OnGetRowAttributes(args);
    }

    /// <summary>
    /// Получение атрибутов ячейки
    /// </summary>
    /// <param name="args"></param>
    protected override void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      FiasActuality actuality;
      if (args.DataRow != null)
      {
        switch (args.ColumnName)
        {
          case "Image":
            if (_UI.DBSettings.UseHistory) // 28.04.2020
            {
              actuality = (FiasActuality)DataTools.GetInt(args.DataRow, "Actuality");
              args.ToolTipText = "Актуальность: " + FiasEnumNames.ToString(actuality);
            }
            else
              actuality = FiasActuality.Actual;
            args.Value = EFPApp.MainImages.Images[EFPFiasListDataGridView.GetImageKey(actuality)];
            break;
          case "Actual":
            actuality = (FiasActuality)DataTools.GetInt(args.DataRow, "Actuality");
            switch (actuality)
            {
              case FiasActuality.Actual:
                args.Value = true;
                break;
              case FiasActuality.Historical:
                args.Value = false;
                break;
              default:
                args.ContentVisible = false;
                break;
            }
            break;
          case "Live":
            if (args.DataRow.IsNull("Live"))
            {
              args.ContentVisible = false;
              args.ColorType = EFPDataGridViewColorType.Warning; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            else
              args.Value = DataTools.GetBool(args.DataRow, "Live");
            break;
        }
      }

      base.OnGetCellAttributes(args);
    }

    #endregion

    #region Просмотр истории

    /// <summary>
    /// Просмотр истории изменений адресного выбранного адресного объекта
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void EFPFiasAddressDetailGridView_EditData(object sender, EventArgs args)
    {
      if (!CheckSingleRow())
        return;

      FiasLevel level = (FiasLevel)DataTools.GetInt(CurrentDataRow, "Level");
      Guid guid = DataTools.GetGuid(CurrentDataRow, "GUID");

      _UI.ShowHistory(FiasTools.GetTableType(level), guid);
    }

    #endregion
  }
}
