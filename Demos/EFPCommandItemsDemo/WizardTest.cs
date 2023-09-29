using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Forms;
using System.Windows.Forms;

namespace EFPCommandItemsDemo
{
  public class WizardTest
  {
    private Wizard _Wizard;

    public void Test()
    {
      WizardStepWithMessage step1 = new WizardStepWithMessage();
      step1.Title = "WizardStepWithMessage";
      step1.Text = "Hello!";
      step1.GetNext += new WizardGetNextEventHandler(step1_GetNext);
      _Wizard = new Wizard(step1);
      _Wizard.Title = "Тест выполнения Мастера";
      _Wizard.Execute();
    }

    void step1_GetNext(object sender, WizardGetNextEventArgs args)
    {
      WizardStepWithGroupBox step2 = new WizardStepWithGroupBox();
      step2.TopLabel.Text = "Текст сверху";
      step2.Title = "WizardStepWithGroupBox";
      step2.BottomLabel.Text = "Текст снизу";
      step2.GetNext += new WizardGetNextEventHandler(step2_GetNext);
      args.NextStep = step2;
    }

    void step2_GetNext(object sender, WizardGetNextEventArgs args)
    {
      WizardStepWithRadioButtons step3 = new WizardStepWithRadioButtons(new string[] { "&1. One", "&2. Two", "&3. Three" });
      step3.Title = "WizardStepWithRadioButtons";
      step3.GetNext += new WizardGetNextEventHandler(step3_GetNext);
      args.NextStep = step3;
    }

    void step3_GetNext(object sender, WizardGetNextEventArgs args)
    {
      WizardStepWithListView step4 = new WizardStepWithListView(new string[] { "One", "Two", "Three" });
      step4.Title = "WizardStepWithListView";
      step4.GetNext += new WizardGetNextEventHandler(step4_GetNext);
      args.NextStep = step4;
    }

    void step4_GetNext(object sender, WizardGetNextEventArgs args)
    {
      WizardStepWithDataGridView step5 = new WizardStepWithDataGridView(delegate(EFPControlWithToolBar<DataGridView> cwt) { return new EFPDataGridView(cwt); });
      step5.Title = "WizardStepWithDataGridView";
      step5.FinalStep = true;
      args.NextStep = step5;
    }
  }
}
