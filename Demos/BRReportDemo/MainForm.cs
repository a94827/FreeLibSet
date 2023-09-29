using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Reporting;

namespace BRReportDemo
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpTest = new EFPRadioButtons(efpForm, rbTest1);
      efpTest.SelectedIndex = 0;

      efpFormatProvider = new EFPListComboBox(efpForm, cbFormatProvider);
      efpFormatProvider.EnabledEx = efpTest[0].CheckedEx;
      _Cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
      string[] cultNames = new string[_Cultures.Length + 1];
      cultNames[0] = "null";
      for (int i = 0; i < _Cultures.Length; i++)
        cultNames[i + 1] = _Cultures[i].ToString();
      efpFormatProvider.Control.Items.AddRange(cultNames);
      efpFormatProvider.Control.SelectedIndex = 0;

      efpConfig = new EFPRadioButtons(efpForm, rbRegistryConfigManager);
      efpConfig.SelectedIndex = 0;

      efpOLEPreferred = new EFPCheckBox(efpForm, cbOLEPreferred);

      btnOk.Image = EFPApp.MainImages.Images["Ok"];
      btnOk.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpOk = new EFPButton(efpForm, btnOk);
      efpOk.Click += EfpOk_Click;

      btnAbout.Image = EFPApp.MainImages.Images["About"];
      btnAbout.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpAbout = new EFPButton(efpForm, btnAbout);
      efpAbout.DisplayName = "О программе";
      efpAbout.Click += EfpAbout_Click;

      lblOSVersion.Text = EnvironmentTools.NetVersionText + Environment.NewLine + EnvironmentTools.OSVersionText;

      _DummyConfigManager = EFPApp.ConfigManager;
    }

    private void EfpAbout_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAboutDialog();
    }

    #endregion

    #region Поля

    EFPRadioButtons efpTest, efpConfig;
    EFPListComboBox efpFormatProvider;
    EFPCheckBox efpOLEPreferred;

    private CultureInfo[] _Cultures;
    private IEFPConfigManager _DummyConfigManager;

    #endregion

    #region Выполнение теста

    private void EfpOk_Click(object sender, EventArgs args)
    {
      if (efpConfig.SelectedIndex == 0)
        EFPApp.ConfigManager = new EFPRegistryConfigManager(@"HKEY_CURRENT_USER\Software\FreeLibSet\BRReportDemo");
      else
        EFPApp.ConfigManager = _DummyConfigManager;

      BRMenuOutItem.OLEPreferred = cbOLEPreferred.Checked;

      switch (efpTest.SelectedIndex)
      {
        case 0:
          IFormatProvider formatProvider = null;
          if (efpFormatProvider.SelectedIndex > 0)
            formatProvider = _Cultures[efpFormatProvider.SelectedIndex - 1];
            Tester.TestBRReport(formatProvider);
          break;
        case 1:
          Tester.TestTreeView(true, false);
          break;
        case 2:
          Tester.TestTreeView(false, false);
          break;
        case 3:
          Tester.TestTreeView(false, true);
          break;
      }
    }


    #endregion
  }
}
