using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.FIAS;
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
#pragma warning disable 1591

  public partial class FiasAddressPanel : UserControl
  {
    #region Конструктор

    public FiasAddressPanel()
    {
      InitializeComponent();

      _VisibleLevels = new Dictionary<FiasLevel, bool>();
      _VisibleLevels.Add(FiasLevel.Region, true);
      _VisibleLevels.Add(FiasLevel.District, true);
      _VisibleLevels.Add(FiasLevel.City, true);
      _VisibleLevels.Add(FiasLevel.Village, true);
      _VisibleLevels.Add(FiasLevel.PlanningStructure, true);
      _VisibleLevels.Add(FiasLevel.Street, true);
      _VisibleLevels.Add(FiasLevel.House, true);
      _VisibleLevels.Add(FiasLevel.Building, true);
      _VisibleLevels.Add(FiasLevel.Structure, true);
      _VisibleLevels.Add(FiasLevel.Flat, true);
      _VisibleLevels.Add(FiasLevel.Room, true);
      _PostalCodeVisible = true;

      MainTLP.SizeChanged += new EventHandler(MainTLP_SizeChanged);
      //MainTLP.BackColor = Color.Yellow;
      //this.BackColor = Color.Green;
      base.MinimumSize = new Size(500, 0);
    }

    void MainTLP_SizeChanged(object sender, EventArgs args)
    {
      this.Size = MainTLP.Size;
    }

    #endregion

    #region Видимость полос

    /// <summary>
    /// Флажки видимости полос.
    /// Нужна отдельная коллекция, так как нельзя использовать Control.Visible до вывода формы на экран
    /// </summary>
    private Dictionary<FiasLevel, bool> _VisibleLevels;

    internal bool GetLevelVisible(FiasLevel level)
    {
      return _VisibleLevels[level];
    }

    internal void SetLevelVisible(FiasLevel level, bool value)
    {
      _VisibleLevels[level] = value;
      int rowIndex;
      switch (level)
      { 
        case FiasLevel.Region:
          lblRegion.Visible = value;
          edRegionName.Visible = value;
          edRegionName.Visible = value;
          btnRegionSel.Visible = value;
          btnRegionClear.Visible = value;
          rowIndex = 0;
          break;
        case FiasLevel.District:
          lblDistrict.Visible = value;
          edDistrictName.Visible = value;
          cbDistrictAOType.Visible = value;
          btnDistrictSel.Visible = value;
          btnDistrictClear.Visible = value;
          rowIndex = 1;
          break;
        case FiasLevel.City:
          lblCity.Visible = value;
          edCityName.Visible = value;
          cbCityAOType.Visible = value;
          btnCitySel.Visible = value;
          btnCityClear.Visible = value;
          rowIndex = 2;
          break;
        case FiasLevel.Village:
          lblVillage.Visible = value;
          edVillageName.Visible = value;
          cbVillageAOType.Visible = value;
          btnVillageSel.Visible = value;
          btnVillageClear.Visible = value;
          rowIndex = 3;
          break;
        case FiasLevel.PlanningStructure:
          lblPS.Visible = value;
          edPlanStrName.Visible = value;
          cbPlanStrAOType.Visible = value;
          btnPSSel.Visible = value;
          btnPSClear.Visible = value;
          rowIndex = 4;
          break;
        case FiasLevel.Street:
          lblStreet.Visible = value;
          edStreetName.Visible = value;
          cbStreetAOType.Visible = value;
          btnStreetSel.Visible = value;
          btnStreetClear.Visible = value;
          rowIndex = 5;
          break;
        case FiasLevel.House:
          lblHouse.Visible = value;
          edHouseName.Visible = value;
          cbHouseAOType.Visible = value;
          btnHouseSel.Visible = value;
          btnHouseClear.Visible = value;
          rowIndex = 6;
          break;
        case FiasLevel.Building:
          edBuildingName.Visible = value;
          cbBuildingAOType.Visible = value;
          rowIndex = 7;
          break;
        case FiasLevel.Structure:
          edStrName.Visible = value;
          cbStrAOType.Visible = value;
          rowIndex = 8;
          break;
        case FiasLevel.Flat:
          lblFlat.Visible = value;
          edFlatName.Visible = value;
          cbFlatAOType.Visible = value;
          btnFlatSel.Visible = value;
          btnFlatClear.Visible = value;
          rowIndex = 9;
          break;
        case FiasLevel.Room:
          edRoomName.Visible = value;
          cbRoomAOType.Visible = value;
          rowIndex = 10;
          break;
        default:
          throw new BugException();
      }

      MainTLP.RowStyles[rowIndex] = new RowStyle(SizeType.Absolute, value ? 
        (btnRegionSel.Height + btnRegionSel.Margin.Bottom) : // = 27 при 96dpi
        0);
    }

    internal bool PostalCodeVisible
    {
      get { return _PostalCodeVisible; }
      set
      {
        if (value == _PostalCodeVisible)
          return;
        _PostalCodeVisible = value;
        cbManualPostalCode.Visible = value;
        edPostalCode.Visible = value;

        MainTLP.RowStyles[11] = new RowStyle(SizeType.Absolute, value ?
          (edPostalCode.Height + edPostalCode.Margin.Bottom) : // = 23 при 96dpi
          0);
      }
    }
    private bool _PostalCodeVisible;

    #endregion
  }
}
