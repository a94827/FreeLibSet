using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.UICore;

namespace WinFormsDemo.FileControlsDemo
{
  public partial class FileControlsForm : Form
  {
    public FileControlsForm()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);
      //efpForm.ConfigSectionName = "FileControls";

      efpTxt1 = new EFPTextBox(efpForm, txt1);
      efpBr1 = new EFPFolderBrowserButton(efpTxt1, br1);
      //efpBr1.ConfigSectionName = "Dir1";

      efpTxt2 = new EFPHistComboBox(efpForm, txt2);
      efpBr2 = new EFPFolderBrowserButton(efpTxt2, br2);
      //efpBr2.ConfigSectionName = "Dir2";

      efpTxt3 = new EFPTextBox(efpForm, txt3);
      efpBr3 = new EFPFolderBrowserButton(efpTxt3, br3);
      //efpBr3.ConfigSectionName = "Dir3";

      efpTxt4 = new EFPTextBox(efpForm, txt4);
      efpBr4 = new EFPFileDialogButton(efpTxt4, br4);
      efpBr4.Filter = "Текстовые файлы|*.txt";
      //efpBr4.ConfigSectionName = "File4";

      efpTxt5 = new EFPHistComboBox(efpForm, txt5);
      efpBr5 = new EFPFileDialogButton(efpTxt5, br5);
      efpBr5.Filter = "Текстовые файлы|*.txt";
      //efpBr5.ConfigSectionName = "File5";

      efpTxt6 = new EFPTextBox(efpForm, txt6);
      efpBr6 = new EFPFileDialogButton(efpTxt6, br6);
      efpBr6.Filter = "Текстовые файлы|*.txt";
      //efpBr6.ConfigSectionName = "File6";

      efpCanBeEmptyMode = new EFPListComboBox(efpForm, cbCanBeEmptyMode);
      efpCanBeEmptyMode.Control.Items.AddRange(Enum.GetNames(typeof(UIValidateState)));

      efpPathValidateMode = new EFPListComboBox(efpForm, cbPathValidateMode);
      string[] pathValidateModeNames = Enum.GetNames(typeof(TestPathMode));
      pathValidateModeNames[(int)TestPathMode.FileExists] += " (только для EFPFileDialogButton)";
      efpPathValidateMode.Control.Items.AddRange(pathValidateModeNames);

      ReadConfig();

      efpCanBeEmptyMode.SelectedIndexEx.ValueChanged += InitControlProps;
      efpPathValidateMode.SelectedIndexEx.ValueChanged += InitControlProps;
      InitControlProps(null, null);

      efpForm.FormClosed += EfpForm_FormClosed;
    }

    EFPTextBox efpTxt1, efpTxt3, efpTxt4, efpTxt6;
    EFPHistComboBox efpTxt2, efpTxt5;
    EFPFolderBrowserButton efpBr1, efpBr2, efpBr3;
    EFPFileDialogButton efpBr4, efpBr5, efpBr6;

    EFPListComboBox efpCanBeEmptyMode, efpPathValidateMode;

    private void EfpForm_FormClosed(object sender, FormClosedEventArgs args)
    {
      if (DialogResult == DialogResult.OK)
      {
        WriteConfig();
        ShowResults();
      }
    }

    private void WriteConfig()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo("FileControls", EFPConfigCategories.UserParams);
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        cfg.SetString("Dir1", efpTxt1.Text);
        cfg.SetHist("Dir2", efpTxt2.HistList);
        cfg.SetString("Dir3", efpTxt3.Text);

        cfg.SetString("File4", efpTxt4.Text);
        cfg.SetHist("File5", efpTxt5.HistList);
        cfg.SetString("File6", efpTxt6.Text);

        cfg.SetEnum<UIValidateState>("CanBeEmptyMode", (UIValidateState)(efpCanBeEmptyMode.SelectedIndex));
        cfg.SetEnum<TestPathMode>("PathValidateMode", (TestPathMode)(efpPathValidateMode.SelectedIndex));
      }
    }

    private void ReadConfig()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo("FileControls", EFPConfigCategories.UserParams);
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
      {
        efpTxt1.Text = cfg.GetString("Dir1");
        efpTxt2.HistList = cfg.GetHist("Dir2");
        efpTxt3.Text = cfg.GetString("Dir3");
        efpTxt4.Text = cfg.GetString("File4");
        efpTxt5.HistList = cfg.GetHist("File5");
        efpTxt6.Text = cfg.GetString("File6");

        efpCanBeEmptyMode.SelectedIndex=(int)(cfg.GetEnum<UIValidateState>("CanBeEmptyMode"));
        efpPathValidateMode.SelectedIndex=(int)(cfg.GetEnum<TestPathMode>("PathValidateMode"));
      }
    }

    private void InitControlProps(object sender, EventArgs args)
    {
      UIValidateState canBeEmptyMode=(UIValidateState)(efpCanBeEmptyMode.SelectedIndex);
      efpTxt1.CanBeEmptyMode = canBeEmptyMode;
      efpTxt2.CanBeEmptyMode = canBeEmptyMode;
      efpTxt4.CanBeEmptyMode = canBeEmptyMode;
      efpTxt5.CanBeEmptyMode = canBeEmptyMode;

      TestPathMode pathValidateMode1 = (TestPathMode)(efpPathValidateMode.SelectedIndex);
      TestPathMode pathValidateMode2= pathValidateMode1;
      if (pathValidateMode1 == TestPathMode.FileExists)
        pathValidateMode1 = TestPathMode.DirectoryExists;

      efpBr1.PathValidateMode = pathValidateMode1;
      efpBr2.PathValidateMode = pathValidateMode1;
      efpBr3.PathValidateMode = pathValidateMode1;

      efpBr4.PathValidateMode = pathValidateMode2;
      efpBr5.PathValidateMode = pathValidateMode2;
      efpBr6.PathValidateMode = pathValidateMode2;
    }

    private void ShowResults()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("Name", typeof(string));
      tbl.Columns.Add("Path", typeof(string));
      tbl.Columns.Add("RelPath", typeof(string));
      tbl.Rows.Add("Dir1", efpBr1.Path.Path, efpBr1.RelPath.Path);
      tbl.Rows.Add("Dir2", efpBr2.Path.Path, efpBr2.RelPath.Path);
      tbl.Rows.Add("Dir3", efpBr3.Path.Path, efpBr3.RelPath.Path);
      tbl.Rows.Add("Dir4", efpBr4.Path.Path, efpBr4.RelPath.Path);
      tbl.Rows.Add("Dir5", efpBr5.Path.Path, efpBr5.RelPath.Path);
      tbl.Rows.Add("Dir6", efpBr6.Path.Path, efpBr6.RelPath.Path);

      SimpleGridForm frm = new SimpleGridForm();
      frm.Text = "Результаты";
      EFPDataGridView gh = new EFPDataGridView(frm.ControlWithToolBar);
      gh.Control.AutoGenerateColumns = false;
      gh.Columns.AddText("Name", true, "Name", 5,4);
      gh.Columns.AddTextFill("Path", true, "Path", 50, 10);
      gh.Columns.AddTextFill("RelPath", true, "RelPath", 50, 10);
      gh.Control.DataSource = tbl.DefaultView;
      EFPApp.ShowDialog(frm, true);
    }

  }
}
