using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace EFPCommandItemsDemo
{
  public partial class Form2 : Form
  {
    public Form2()
    {
      InitializeComponent();

      Program.LastFormNumber++;
      _FormNumber = Program.LastFormNumber;

      Text = "Форма №" + _FormNumber.ToString();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efp1 = new EFPTextBox(efpForm, ed1);
      efp1.CommandItems.UseStatusBarRC = true;
      efp2 = new EFPIntEditBox(efpForm, ed2);
      efp3 = new EFPDateTimeBox(efpForm, ed3);

      EFPButton efpShowDialogButton = new EFPButton(efpForm, btnShowDialog);
      efpShowDialogButton.Click += new EventHandler(efpShowDialogButton_Click);
    }

    void efpShowDialogButton_Click(object sender, EventArgs args)
    {
      EFPApp.ShowDialog(new Form2(), true);
    }

    int _FormNumber;

    EFPTextBox efp1;
    EFPIntEditBox efp2;
    EFPDateTimeBox efp3;
  }
}
