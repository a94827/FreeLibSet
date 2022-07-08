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
  public partial class Form3 : Form
  {
    public Form3()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      EFPTextBox efpText = new EFPTextBox(efpForm, edText1);
    }
  }
}