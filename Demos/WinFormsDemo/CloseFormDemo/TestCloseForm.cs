using FreeLibSet.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsDemo.CloseFormDemo
{
  public partial class TestCloseForm : Form
  {
    public TestCloseForm()
    {
      InitializeComponent();

      _GlobalCounter++;
      _FormNo = _GlobalCounter;

      Text = "#" + _FormNo.ToString();

      efpForm = new EFPFormProvider(this);
      efpForm.FormClosing += EfpForm_FormClosing;

      cbDialogResult.Items.AddRange(Enum.GetNames(typeof(DialogResult)));
      efpDialogResult = new EFPListComboBox(efpForm, cbDialogResult);
      efpDialogResult.SelectedIndex = 0;

      efpBanner = new EFPCheckBox(efpForm, cbBanner);
      efpBanner.Validating += EfpBanner_Validating;

      efpDelay = new EFPInt32EditBox(efpForm, edDelay);

      EFPButton efpAction = new EFPButton(efpForm, btnAction);
      efpAction.Click += EfpAction_Click;
    }


    private static int _GlobalCounter = 0;
    private int _FormNo;

    EFPFormProvider efpForm;
    public EFPListComboBox efpDialogResult;
    public EFPCheckBox efpBanner;
    public EFPInt32EditBox efpDelay;

    private void EfpBanner_Validating(object sender, FreeLibSet.UICore.UIValidatingEventArgs args)
    {
      if (efpBanner.Checked)
        args.SetError("Form closing is prohibited");
    }

    private void EfpForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      int delay = efpDelay.Value;
      if (delay > 0)
      {
        using (Splash spl = new Splash("Delaying form #"+_FormNo.ToString()+" closing"))
        {
          spl.AllowCancel = true;
          spl.PercentMax = delay * 10;
          for (int i = 0; i < delay * 10; i++)
          {
            Thread.Sleep(100);
            spl.IncPercent();
          }
        }
      }
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
      EFPApp.MessageBox("FormClosed #" + _FormNo.ToString());
    }

    private void EfpAction_Click(object sender, EventArgs args)
    {
      DialogResult dlgRes = (DialogResult)(Enum.Parse(typeof(DialogResult), efpDialogResult.SelectedItemString));

      // Тестируемый метод
      bool res = efpForm.CloseForm(dlgRes);

      if (res)
      {
        EFPApp.MessageBox("Success! DialogResult=" + this.DialogResult.ToString() + Environment.NewLine + "Let's try again.");
        EFPApp.ShowDialog(new TestCloseForm(), true);
      }
      else
      {
        EFPApp.ErrorMessageBox("Cannot close");
      }
    }

  }
}
