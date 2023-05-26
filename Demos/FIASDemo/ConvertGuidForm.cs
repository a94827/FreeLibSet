using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Forms;

namespace FIASDemo
{
  public partial class ConvertGuidForm : Form
  {
    #region Конструктор формы

    public ConvertGuidForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpMode = new EFPRadioButtons(efpForm, rbGuid2Bin);

      edGuid.Mask = @"&&&&&&&&\-&&&&\-&&&&\-&&&&\-&&&&&&&&&&&&";
      efpGuid = new EFPMaskedTextBox(efpForm, edGuid);
      efpGuid.CanBeEmpty = true;

      edBin.Mask = @"&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&";
      efpBin = new EFPMaskedTextBox(efpForm, edBin);
      efpBin.CanBeEmpty = true;

      efpMode.SelectedIndexEx.ValueChanged += new EventHandler(AnythingChanged);
      efpGuid.TextEx.ValueChanged += new EventHandler(AnythingChanged);
      efpBin.TextEx.ValueChanged += new EventHandler(AnythingChanged);
      AnythingChanged(null, null);
    }

    #endregion

    #region Поля

    EFPRadioButtons efpMode;

    EFPMaskedTextBox efpGuid, efpBin;

    #endregion

    #region Преобразование

    void AnythingChanged(object sender, EventArgs args)
    {
      efpGuid.ReadOnly = efpMode.SelectedIndex != 0;
      efpBin.ReadOnly = efpMode.SelectedIndex == 0;
      if (efpMode.SelectedIndex == 0)
        efpBin.Text = GuidToBin(efpGuid.Text);
      else
        efpGuid.Text = BinToGuid(efpBin.Text);
    }

    [DebuggerStepThrough]
    private static string GuidToBin(string sGuid)
    {
      try
      {
        Guid g = new Guid(sGuid);
        byte[] a = g.ToByteArray();
        return DataTools.BytesToHex(a, false);
      }
      catch
      {
        return String.Empty;
      }
    }

    [DebuggerStepThrough]
    private string BinToGuid(string sBytes)
    {
      try
      {
        byte[] a = DataTools.HexToBytes(sBytes);
        Guid g = new Guid(a);
        return g.ToString("D");
      }
      catch
      {
        return String.Empty;
      }
    }


    #endregion
  }
}
