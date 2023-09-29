using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRPageSetupPrinter : Form
  {
    public BRPageSetupPrinter()
    {
      InitializeComponent();
    }
  }


  #region Закладка "Принтер"
#if XXX
  private class PrinterTab : IAccDepPageSetupTab
  {
    #region Конструктор

    public PrinterTab(BRPageSetupTabs Form)
    {
      FForm = Form;
    }

    BRPageSetupTabs FForm;

    #endregion

    #region IAccDepPageSetupTab Members

    public string Title { get { return "Принтер"; } }

    public string ToolTipText { get { return "Выбор принтера"; } }

    public string ImageKey { get { return "Print"; } }

    public void InitTab(Control ParentControl, EFPBaseProvider BaseProvider)
    {
      ParentControl.Controls.Add(FForm.panPrinter);

      efpPrinterMode = new EFPRadioButtons(BaseProvider, FForm.rbDefaultPrinter);
      efpPrinterMode.DisplayName = "Режим выбора принтера";
      efpPrinterMode.ToolTipText = "Выбор между использованием принтера по умолчанию\r\n" +
        "(который может изменяться средствами Windows) и жестко закрепленным принтером";
      efpPrinterMode.SelectedIndexEx.ValueChanged += new EventHandler(InitPrinterInfo);

      new PrinterComboBoxFiller(FForm.cbPrinter);
      efpPrinterList = new EFPTextComboBox(BaseProvider, FForm.cbPrinter);
      efpPrinterList.DisplayName = "Название принтера";
      efpPrinterList.ToolTipText = "Принтер, который будет всегда использоваться для печати этого задания";
      efpPrinterList.TextEx.ValueChanged += new EventHandler(InitPrinterInfo);
      efpPrinterList.AllowDisabledText = false;
      efpPrinterList.EnabledEx = efpPrinterMode[1].CheckedEx;
      efpPrinterList.CanBeEmpty = false;

      FForm.btnPrinterProps.Image = EFPApp.MainImages.Images["Properties"];
      FForm.btnPrinterProps.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpBtnPrinterProps = new EFPButton(BaseProvider, FForm.btnPrinterProps);
      efpBtnPrinterProps.Click += new EventHandler(efpBtnPrinterProps_Click);
      efpBtnPrinterProps.Visible = false; // не работает

      EFPButton efpBtnDebug = new EFPButton(BaseProvider, FForm.btnDebug);
      efpBtnDebug.Visible = AccDepClientExec.DebugShowIds;
      efpBtnDebug.Click += new EventHandler(efpBtnDebug_Click);
    }

    public void ValuesToControl()
    {
      InsideInitPrinterInfo = true;
      if (String.IsNullOrEmpty(FForm.PageSetup.PrinterName))
      {
        efpPrinterMode.SelectedIndex = 0;
        efpPrinterList.Text = EFPApp.Printers.DefaultPrinterName;
      }
      else
      {
        efpPrinterMode.SelectedIndex = 1;
        efpPrinterList.Text = FForm.PageSetup.PrinterName;
      }
      efpPrinterList.DisabledText = EFPApp.Printers.DefaultPrinterName;
      InsideInitPrinterInfo = false;
      InitPrinterInfo(null, null);

      if (FForm.PageSetup.AllowBackground)
      {
        if (EFPApp.BackgroundPrinting.Enabled)
          FForm.lblBackInfo.Text = "Включена";
        else
          FForm.lblBackInfo.Text = "Отключена";
      }
      else
        FForm.lblBackInfo.Text = "Не поддерживается";
    }

    public void ValuesFromControl()
    {
    }

    #endregion

    EFPRadioButtons efpPrinterMode;
    EFPTextComboBox efpPrinterList;

    private void InitPrinterInfo(object Sender, EventArgs Args)
    {
      if (InsideInitPrinterInfo)
        return;
      string PrinterName = null;
      if (efpPrinterMode[1].Checked)
        PrinterName = efpPrinterList.Text;
      //   if (PrinterName == FForm.PageSetup.PrinterName)
      //     return;
      InsideInitPrinterInfo = true;
      try
      {
        FForm.PageSetup.PrinterName = PrinterName;

        //if (!rbSelPrinter.CheckedEx)
        //  cbPrinter.Text = PrinterInfo.PrinterName;

        if (FForm.PageSetup.PrinterInfo.IsValid)
          FForm.lblPrinterInfo.Visible = false;
        else
        {
          if (String.IsNullOrEmpty(PrinterName))
            FForm.lblPrinterInfo.Text = "Принтер не выбран";
          else
            FForm.lblPrinterInfo.Text = "Неправильное имя принтера";
          FForm.lblPrinterInfo.Visible = true;
        }
        //InitDuplexCheckBox();
      }
      finally
      {
        InsideInitPrinterInfo = false;
      }
    }
    private bool InsideInitPrinterInfo = false;

    /// <summary>
    /// Вывод диалога свойств принтера
    /// </summary>
    private void efpBtnPrinterProps_Click(object Sender, EventArgs Args)
    {
      EFPApp.MessageBox("Не реализовано");
    }

    private void efpBtnDebug_Click(object Sender, EventArgs Args)
    {
      DebugTools.DebugObject(FForm.PageSetup, "Параметры страницы");
    }
  }

#endif
  #endregion

}
