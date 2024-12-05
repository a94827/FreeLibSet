using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace WinFormsDemo.WizardDemo
{
  public partial class WizardTemplateForm : Form
  {
    public WizardTemplateForm()
    {
      InitializeComponent();

      Cfg = new RegistryCfg(Program.RegKeyName+@"\Wizard");
      InitStep1();
      _TheWizard = new Wizard(Step1);
    }


    public Wizard TheWizard { get { return _TheWizard; } }
    private Wizard _TheWizard;


    private CfgPart Cfg;

    #region Выбор режима

    private WizardStepWithRadioButtons Step1;

    private void InitStep1()
    {
      if (Step1 == null)
      {
        string[] a = new string[]
        {
          "Использование формы-шаблона",
          "Свойства WizardStep",
          "ExtWizardStep",
          "WizardStepWithTabControl",
          "TempPage",
        };

        Step1 = new WizardStepWithRadioButtons(a);
        Step1.GroupTitle = "Режим работы";
        Step1.TheButtons.Codes = new string[] { "TemplateForm", "WizardStepProps", "ExtWizardStep", "WizardStepWithTabControl", "TempPage"};

        Step1.EndStep += Step1_EndStep;
        Step1.GetNext += Step1_GetNext;

        try { Step1.TheButtons.SelectedCode = Cfg.GetString("TestMode"); }
        catch { }
      }
    }

    private void Step1_EndStep(object sender, WizardEndStepEventArgs args)
    {
      Cfg.SetString("TestMode", Step1.TheButtons.SelectedCode);
    }

    private void Step1_GetNext(object sender, WizardGetNextEventArgs args)
    {
      switch (Step1.TheButtons.SelectedCode)
      {
        case "TemplateForm":
          InitStep101();
          args.NextStep = Step101;
          break;
        case "WizardStepProps":
          InitStep501();
          args.NextStep = Step501;
          break;
        case "ExtWizardStep":
          InitStep201();
          args.NextStep = Step201;
          break;
        case "WizardStepWithTabControl":
          InitStep401();
          args.NextStep = Step401;
          break;
        case "TempPage":
          InitStep301();
          args.NextStep = Step301;
          break;
        default:
          throw new BugException();
      }
    }

    #endregion

    #region Использование формы-шаблона

    private WizardStep Step101;

    private EFPTextBox efpTest101;

    private void InitStep101()
    {
      if (Step101 == null)
      {
        Step101 = new WizardStep(Pan101);

        Step101.BeginStep += Step101_BeginStep;
        Step101.EndStep += Step101_EndStep;
        Step101.FinalStep = true;

        efpTest101 = new EFPTextBox(Step101.BaseProvider, edTest101);

        try { efpTest101.Text = Cfg.GetString("TestValue101"); }
        catch { }
      }
    }

    private void Step101_BeginStep(object sender, WizardBeginStepEventArgs args)
    {
    }

    private void Step101_EndStep(object sender, WizardEndStepEventArgs args)
    {
      if (args.Forward)
      {
        Cfg.SetString("TestValue101", efpTest101.Text);
      }
    }

    #endregion

    #region Свойства WizardStep

    private WizardStep Step501;

    private EFPCheckBox efpFinalStep501, efpForwardEnabled501, efpBackEnabled501, efpTitle501, efpTitleForThisStepOnly501, efpHelpContext501;

    private void InitStep501()
    {
      if (Step501 == null)
      {
        Step501 = new WizardStep(Pan501);
        Step501.GetNext += Step501_GetNext;

        efpFinalStep501 = new EFPCheckBox(Step501.BaseProvider, cbFinalStep501);
        efpForwardEnabled501 = new EFPCheckBox(Step501.BaseProvider, cbForwardEnabled501);
        efpBackEnabled501 = new EFPCheckBox(Step501.BaseProvider, cbBackEnabled501);
        efpTitle501 = new EFPCheckBox(Step501.BaseProvider, cbTitle501);
        efpTitleForThisStepOnly501 = new EFPCheckBox(Step501.BaseProvider, cbTitleForThisStepOnly501);
        efpHelpContext501 = new EFPCheckBox(Step501.BaseProvider, cbHelpContext501);

        efpFinalStep501.Checked = Step501.FinalStep;
        efpForwardEnabled501.Checked = Step501.ForwardEnabled;
        efpBackEnabled501.Checked = Step501.BackEnabled;
        efpTitle501.Checked = !String.IsNullOrEmpty(Step501.Title);
        efpTitleForThisStepOnly501.Checked = Step501.TitleForThisStepOnly;
        efpHelpContext501.Checked = !String.IsNullOrEmpty(Step501.HelpContext);

        efpFinalStep501.CheckedEx.ValueChanged += efpFinalStep501_ValueChanged; 
        efpForwardEnabled501.CheckedEx.ValueChanged += efpForwardEnabled501_ValueChanged;
        efpBackEnabled501.CheckedEx.ValueChanged += efpBackEnabled501_ValueChanged;
        efpTitle501.CheckedEx.ValueChanged += efpTitle501_ValueChanged;
        efpTitleForThisStepOnly501.CheckedEx.ValueChanged += efpTitleForThisStepOnly501_ValueChanged;
        efpHelpContext501.CheckedEx.ValueChanged += efpHelpContext501_ValueChanged;
      }
    }

    private void efpFinalStep501_ValueChanged(object sender, EventArgs args)
    {
      Step501.FinalStep = efpFinalStep501.Checked;
    }

    private void efpForwardEnabled501_ValueChanged(object sender, EventArgs args)
    {
      Step501.ForwardEnabled = efpForwardEnabled501.Checked;
    }

    private void efpBackEnabled501_ValueChanged(object sender, EventArgs args)
    {
      Step501.BackEnabled = efpBackEnabled501.Checked;
    }

    private void efpTitle501_ValueChanged(object sender, EventArgs args)
    {
      if (efpTitle501.Checked)
        Step501.Title = "Особый заголовок";
      else
        Step501.Title = String.Empty;
    }

    private void efpTitleForThisStepOnly501_ValueChanged(object sender, EventArgs args)
    {
      Step501.TitleForThisStepOnly = efpTitleForThisStepOnly501.Checked;
    }

    private void efpHelpContext501_ValueChanged(object sender, EventArgs args)
    {
      if (efpHelpContext501.Checked)
        Step501.HelpContext = "Step501";
      else
        Step501.HelpContext = String.Empty;
    }

    private void Step501_GetNext(object sender, WizardGetNextEventArgs args)
    {
      InitStep502();
      args.NextStep = Step502;
    }

    private WizardStepWithMessage Step502;

    private void InitStep502()
    {
      if (Step502 == null)
      {
        Step502 = new WizardStepWithMessage();
        Step502.Text = "Промежуточный шаг";
        Step502.GetNext += Step502_GetNext;
      }
    }

    private void Step502_GetNext(object sender, WizardGetNextEventArgs args)
    {
      InitStep503();
      args.NextStep = Step503;
    }

    private WizardStepWithMessage Step503;

    private void InitStep503()
    {
      if (Step503 == null)
      {
        Step503 = new WizardStepWithMessage();
        Step503.Text = "Финальный шаг";
        Step503.FinalStep = true;
      }
    }

    #endregion

    #region ExtWizardStep

    private WizardStep Step201;

    private EFPCheckBox efpGroupTitle201;
    private EFPListComboBox efpInfoText201, efpInfoIcon201, efpInfoIconSize201;

    private void InitStep201()
    {
      if (Step201 == null)
      {
        Step201 = new WizardStep(Pan201);
        Step201.GetNext += Step201_GetNext;

        efpGroupTitle201 = new EFPCheckBox(Step201.BaseProvider, cbGroupTitle201);
        efpInfoText201 = new EFPListComboBox(Step201.BaseProvider, cbInfoText201);
        efpInfoText201.SelectedIndex = 0;
        efpInfoIcon201 = new EFPListComboBox(Step201.BaseProvider, cbInfoIcon201);
        efpInfoIcon201.SelectedIndex = 0;
        efpInfoIconSize201 = new EFPListComboBox(Step201.BaseProvider, cbInfoIconSize201);
        efpInfoIconSize201.SelectedIndex = 0;
      }
    }

    private void Step201_GetNext(object sender, WizardGetNextEventArgs args)
    {
      InitStep202();
      args.NextStep = Step202;
    }

    private ExtWizardStep Step202;

    private void InitStep202()
    {
      if (Step202 == null)
      {
        Step202 = new ExtWizardStep();

        TextBox tb = new TextBox();
        tb.Multiline = true;
        tb.Text = "Hello, world!";
        tb.ReadOnly = true;
        tb.Dock = DockStyle.Fill;

        Step202.MainPanel.Controls.Add(tb);

        Step202.BeginStep += Step202_BeginStep;
        Step202.FinalStep = true;
      }
    }

    private Random rnd = new Random();

    private void Step202_BeginStep(object sender, WizardBeginStepEventArgs args)
    {
      if (efpGroupTitle201.Checked)
        Step202.GroupTitle = "Это заголовок группы";
      else
        Step202.GroupTitle = String.Empty;

      switch (efpInfoText201.SelectedIndex)
      {
        case 0:
          Step202.InfoText = String.Empty;
          break;
        case 1:
          Step202.InfoText = "Короткий текст";
          break;
        case 2:
          StringBuilder sb = new StringBuilder();
          int n = 10 + rnd.Next(30);
          for (int i = 0; i < n; i++)
            sb.Append("Длинный ");
          sb.Append("текст");
          Step202.InfoText = sb.ToString();
          break;
      }

      Step202.InfoIcon = StdConvert.ToEnum<MessageBoxIcon>(efpInfoIcon201.SelectedItemString);
      Step202.InfoIconSize = StdConvert.ToEnum<MessageBoxIconSize>(efpInfoIconSize201.SelectedItemString);
    }

    #endregion

    #region WizardStepWithTabControl

    private WizardStepWithTabControl Step401;

    private void InitStep401()
    {
      if (Step401 == null)
      {
        Step401 = new WizardStepWithTabControl();
        Step401.FinalStep = true;

        EFPTabPage tp1 = Step401.TheTabControl.TabPages.Add("Сообщения");
        EFPControlWithToolBar<DataGridView> cwt1 = new EFPControlWithToolBar<DataGridView>(tp1);
        EFPErrorDataGridView efp1 = new EFPErrorDataGridView(cwt1);
        efp1.ControlledTabPageControl = tp1;

        ErrorMessageList msgs = new ErrorMessageList();
        msgs.AddWarning("Тестовое сообщение");
        efp1.ErrorMessages = msgs;

        EFPTabPage tp2 = Step401.TheTabControl.TabPages.Add("Текст");
        EFPControlWithToolBar<TextBox> cwt2 = new EFPControlWithToolBar<TextBox>(tp2, "Текстовое поле");
        cwt2.Control.Multiline = true;
        EFPTextBox efp2 = new EFPTextBox(cwt2);
        efp2.CanBeEmpty = false;
      }
    }

    #endregion

    #region TempPage

    private WizardStepWithRadioButtons Step301;

    private void InitStep301()
    {
      if (Step301 == null)
      {
        Step301 = new WizardStepWithRadioButtons(new string[] { "BeginStep", "EndStep" });
        Step301.GroupTitle = "Когда показать заставку";
        Step301.ItemInfoTextArray = new string[] {
          "Действие выполняется внутри обработчика события BeginStep. Прерывание действия не предотвращает переход к следующему кадру",
          "Действие выполняется внутри обработчика события EndStep. Прерывание действия оставляет мастер на текущем кадре"};
        Step301.EndStep += Step301_EndStep;
        Step301.GetNext += Step301_GetNext;
      }
    }

    private void Step301_EndStep(object sender, WizardEndStepEventArgs args)
    {
      if (args.Forward)
      {
        if (Step301.TheButtons.SelectedIndex == 1)
          ProcessTempPage(args.Wizard);
      }
    }

    private void ProcessTempPage(Wizard wizard)
    {
      ISplash spl = wizard.BeginTempPage("Длительный процесс");
      try
      {
        spl.AllowCancel = true;
        spl.PercentMax = 100;
        for (int i = 0; i < 100; i++)
        {
          spl.Sleep(100);
          spl.IncPercent();
        }
      }
      finally
      {
        wizard.EndTempPage();
      }
    }

    private void Step301_GetNext(object sender, WizardGetNextEventArgs args)
    {
      WizardStepWithMessage step302 = new WizardStepWithMessage();
      step302.Text = "Длительный процесс выполнен";
      step302.BeginStep += Step302_BeginStep;
      step302.FinalStep = true;
      args.NextStep = step302;
    }

    private void Step302_BeginStep(object sender, WizardBeginStepEventArgs args)
    {
      if (args.Forward)
      {
        if (Step301.TheButtons.SelectedIndex == 0)
          ProcessTempPage(args.Wizard);
      }
    }

    #endregion
  }
}
