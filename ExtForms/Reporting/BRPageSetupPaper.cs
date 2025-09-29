using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Reporting;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRPageSetupPaper : Form
  {
    #region Конструктор

    public BRPageSetupPaper(SettingsDialog dialog)
    {
      InitializeComponent();

      SettingsDialogPage page = dialog.Pages.Add(panPaper);

      InitSizeTable();

      //grpPaperSize.Visible = FForm.PageSetup.AllowPaper;
      efpPageSize = new EFPListComboBox(page.BaseProvider, cbPageSize);
      efpPageSize.ToolTipText = Res.BRPageSetupPaper_ToolTip_PageSize;
      efpPageSize.SelectedIndexEx.ValueChanged += new EventHandler(PageSizeChanged);

      efpPaperHeight = new EFPDecimalEditBox(page.BaseProvider, edPaperHeight);
      efpPaperHeight.ToolTipText = Res.BRPageSetupPaper_ToolTip_PaperHeight;
      efpPaperHeight.Control.Increment = 0.1m;
      efpPaperHeight.ValueEx.ValueChanged += new EventHandler(PaperWidthOrHeightChanged);

      efpPaperWidth = new EFPDecimalEditBox(page.BaseProvider, edPaperWidth);
      efpPaperWidth.ToolTipText = Res.BRPageSetupPaper_ToolTip_PaperWidth;
      efpPaperWidth.Control.Increment = 0.1m;
      efpPaperWidth.ValueEx.ValueChanged += new EventHandler(PaperWidthOrHeightChanged);

      //grpOrientation.Visible = FForm.PageSetup.AllowPaper;
      efpOrientation = new EFPRadioButtons(page.BaseProvider, rbPortrait);
      efpOrientation.ToolTipText = Res.BRPageSetupPaper_ToolTip_Orientation;
      efpOrientation.SelectedIndexEx.ValueChanged += new EventHandler(OrientationChanged);

      //// TODO: grpDuplex.Visible = FForm.PageSetup.AllowDuplex;
      //efpDuplex = new EFPCheckBox(page.BaseProvider, cbDuplex);
      //efpDuplex.ToolTipText = "Режим двусторонней печати (если поддерживается принтером)." + Environment.NewLine +
      //  "Некоторые драйверы принтера поддерживают эмуляцию двусторонней печати путем" + Environment.NewLine +
      //  "выдачи приглашения перевернуть бумагу";
      //efpDuplex.CheckedEx.ValueChanged += new EventHandler(DuplexChanged);

      //grpCenterPage.Visible = FForm.PageSetup.AllowCenterPage;
      efpCenterHorizontal = new EFPCheckBox(page.BaseProvider, cbCenterHorizontal);
      efpCenterHorizontal.ToolTipText = Res.BRPageSetupPaper_ToolTip_CenterHorizontally;

      efpCenterVertical = new EFPCheckBox(page.BaseProvider, cbCenterVertical);
      efpCenterVertical.ToolTipText = Res.BRPageSetupPaper_ToolTip_CenterVertically;

      page.Text = Res.BRPageSetupPaper_Title_Tab;
      page.ToolTipText = Res.BRPageSetupPaper_ToolTip_Tab;
      page.ImageKey = "PaperSize";
      page.DataToControls += DataToControls;
      page.DataFromControls += DataFromControls;
    }

    #endregion

    #region Размер бумаги

    EFPListComboBox efpPageSize;
    EFPDecimalEditBox efpPaperWidth;
    EFPDecimalEditBox efpPaperHeight;

    /// <summary>
    /// Размеры бумаги
    /// </summary>
    private DataTable _SizeTable;
    private DataView _DVByName;
    private DataView _DVByWH;


    private void InitSizeTable()
    {
      // В Mono версии 6.12.0 размеры бумаги отличаются
      // Например, для формата A4 в Windows: 1169*827, а в Mono без Wine: 1169*826
      // Можно было бы делать 2 попытки, одна для функции Round(), а другая - для Trunc(), но этого может быть недостаточно.
      // Размер бумаги задается с точностью 0.1мм, но при выборе стандартного размера используется окргугление до 1мм.
      // Единицей измерения в PaperSettings является 1/100 дюйма, то есть, примерно, 1/4мм.
      // Выполняем поиск с точностью до 4 единиц


      _SizeTable = new DataTable();
      _SizeTable.Columns.Add("Width", typeof(int)); // в миллиметрах
      _SizeTable.Columns.Add("Height", typeof(int));
      _SizeTable.Columns.Add("Name", typeof(string));

      EFPPrinterInfo pi = EFPApp.Printers.DefaultPrinter;
      if (pi != null)
      {
        foreach (PaperSize sz in pi.PaperSizes)
        {
          int h = (int)Math.Round(sz.Height / 100m * 25.4m, 0, MidpointRounding.AwayFromZero); // округляем до 1 мм
          int w = (int)Math.Round(sz.Width / 100m * 25.4m, 0, MidpointRounding.AwayFromZero);
          if (sz.PaperName == "A4")
          {
          }

          _SizeTable.Rows.Add(w, h, sz.PaperName);
        }
      }
      _SizeTable.AcceptChanges();
      _DVByName = _SizeTable.DefaultView;
      _DVByName.Sort = "Name";
      _DVByWH = new DataView(_SizeTable);
      _DVByWH.Sort = "Width,Height";
#if DEBUG
      if (_DVByWH.Count != _DVByName.Count)
        throw new BugException("DVByWH.Count");
#endif

      string[] aNames = new string[_DVByName.Count + 1];
      for (int i = 0; i < _DVByName.Count; i++)
        aNames[i] = _DVByName[i].Row["Name"].ToString();
      aNames[_DVByName.Count] = Res.BRPageSetupPaper_Msg_NonStandard;
      cbPageSize.Items.Clear();
      cbPageSize.Items.AddRange(aNames);
    }

    /// <summary>
    /// Флажок для блокировки рекурсивного изменения размеров
    /// </summary>
    private bool _InsidePaperSize = false;

    private void PageSizeChanged(object sender, EventArgs args)
    {
      if (_InsidePaperSize)
        return;
      _InsidePaperSize = true;
      try
      {
        if (efpPageSize.SelectedIndex >= 0 && efpPageSize.SelectedIndex < (_DVByName.Count - 1))
        {
          DataRow row = _DVByName[efpPageSize.SelectedIndex].Row;
          int h = DataTools.GetInt32(row, "Height"); // в единицах 1мм
          int w = DataTools.GetInt32(row, "Width");
          if (efpOrientation.SelectedIndex == 1)
            DataTools.Swap<int>(ref h, ref w);
          efpPaperHeight.Value = h / 10m;
          efpPaperWidth.Value = w / 10m;
        }
      }
      finally
      {
        _InsidePaperSize = false;
      }
    }

    private void PaperWidthOrHeightChanged(object sender, EventArgs args)
    {
      if (_InsidePaperSize)
        return;

      _InsidePaperSize = true;
      try
      {
        int h = (int)(Math.Round(efpPaperHeight.Value, 1, MidpointRounding.AwayFromZero) * 10m); // в единицах 1мм
        int w = (int)(Math.Round(efpPaperWidth.Value, 1, MidpointRounding.AwayFromZero) * 10m);
        if (efpOrientation.SelectedIndex == 1)
          DataTools.Swap<int>(ref h, ref w);

        int p = _DVByWH.Find(new object[2] { w, h });
        if (p >= 0)
        {
          DataRow row = _DVByWH[p].Row;
          efpPageSize.SelectedIndex = DataTools.FindDataRowViewIndex(_DVByName, row);
        }
        else
        {
          efpPageSize.SelectedIndex = _DVByName.Count;
          //efpPageSize.Control.Text = "Другой";
        }

      }
      finally
      {
        _InsidePaperSize = false;
      }
    }

    #endregion

    #region Двусторонняя печать
#if XXX
    EFPCheckBox efpDuplex;

    /// <summary>
    /// Инициализация двусторонней печати
    /// </summary>
    private void InitDuplexCheckBox()
    {
#if XXX
      if (efpDuplex == null)
        return; // досрочный вызов
      efpDuplex.Enabled = FForm.PageSetup.AllowDuplex && FForm.PageSetup.PrinterInfo.CanDuplex;
      if (efpDuplex.Enabled)
        efpDuplex.Checked = FForm.PageSetup.Duplex;
      else
        efpDuplex.Checked = false;
      FForm.lblDuplexInfo.Visible = !FForm.cbDuplex.Enabled;
      if (FForm.PageSetup.AllowDuplex)
      {
        if (FForm.PageSetup.PrinterInfo.IsValid)
        {
          if (FForm.PageSetup.PrinterInfo.CanDuplex)
            FForm.lblDuplexInfo.Text = "";
          else
            FForm.lblDuplexInfo.Text = "Принтер \"" + FForm.PageSetup.PrinterInfo.PrinterName +
              "\" не поддерживает двустороннюю печать";
        }
        else
        {
          FForm.lblDuplexInfo.Text = "Задано неправильное имя принтера";
        }
      }
      else
      {
        FForm.lblDuplexInfo.Text = "Задание должно печататься на одной стороне бумаги";
      }
#endif
    }

    private void DuplexChanged(object sender, EventArgs args)
    {
#if XXX
      if (!FForm.cbDuplex.Enabled)
        return;
      FForm.PageSetup.Duplex = FForm.cbDuplex.Checked;
#endif
    }
#endif

    #endregion

    #region Центрирование

    EFPCheckBox efpCenterHorizontal, efpCenterVertical;

    #endregion

    #region Ориентация бумаги

    EFPRadioButtons efpOrientation;

    private void InitOrientationImage()
    {
      if (efpOrientation.SelectedIndex == 1)
        pbOrientation.Image = pbSrcLandscape.Image;
      else
        pbOrientation.Image = pbSrcPortrait.Image;
    }

    private void OrientationChanged(object sender, EventArgs args)
    {
      // обмениваем ширину и высоту
      decimal tmp = efpPaperWidth.Value;
      efpPaperWidth.Value = efpPaperHeight.Value;
      efpPaperHeight.Value = tmp;

      InitOrientationImage();
      PaperWidthOrHeightChanged(null, null);
    }

    #endregion

    #region Обработчики SettingsDialogPage

    private void DataToControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRPageSetup ps = page.Owner.Data.GetItem<BRPageSettingsDataItem>().PageSetup;
      _InsidePaperSize = true;
      try
      {
        efpOrientation.SelectedIndex = (int)(ps.Orientation);
        efpPaperHeight.Value = Math.Round(ps.PaperHeight / 100m, 2, MidpointRounding.AwayFromZero);
        efpPaperWidth.Value = Math.Round(ps.PaperWidth / 100m, 2, MidpointRounding.AwayFromZero);
      }
      finally
      {
        _InsidePaperSize = false;
      }
      PaperWidthOrHeightChanged(null, null);

      efpCenterHorizontal.Checked = ps.CenterHorizontal;
      efpCenterVertical.Checked = ps.CenterVertical;

      //InitDuplexCheckBox();


      //if (efpDuplex.Editable)
      //  efpDuplex.Checked=ps.di
    }

    private void DataFromControls(object sender, EventArgs args)
    {
      SettingsDialogPage page = (SettingsDialogPage)sender;
      BRPageSetup ps = page.Owner.Data.GetItem<BRPageSettingsDataItem>().PageSetup;
      ps.SetOrientation((BROrientation)(efpOrientation.SelectedIndex), true);
      ps.PaperHeight = (int)(Math.Round(efpPaperHeight.Value * 100m, 0, MidpointRounding.AwayFromZero));
      ps.PaperWidth = (int)(Math.Round(efpPaperWidth.Value * 100m, 0, MidpointRounding.AwayFromZero));
      ps.CenterHorizontal = efpCenterHorizontal.Checked;
      ps.CenterVertical = efpCenterVertical.Checked;
    }

    #endregion

  }
}
