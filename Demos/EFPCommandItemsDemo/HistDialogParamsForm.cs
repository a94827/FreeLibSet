using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.DependedValues;
using FreeLibSet.Forms;
using FreeLibSet.IO;

namespace EFPCommandItemsDemo
{
  public partial class HistDialogParamsForm : Form
  {
    #region Конструктор формы

    public HistDialogParamsForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Debug"];

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpDialog = new EFPRadioButtons(efpForm, rbHistFolderBrowserDialog);
      efpDialog.SelectedIndex = 0;

      efpHistList = new EFPTextBox(efpForm, edHistList);
      efpHistList.CanBeEmpty = true;

      efpConfigSectionName = new EFPTextBox(efpForm, edCongfigSectionName);
      efpConfigSectionName.CanBeEmpty = true;

      efpMaxHistLength = new EFPIntEditBox(efpForm, edMaxHistLength);
      efpMaxHistLength.CanBeEmpty = false;
      efpMaxHistLength.Minimum = EFPHistComboBox.MinimumMaxHistLength;
      efpMaxHistLength.Maximum = EFPHistComboBox.MaximumMaxHistLength;
      efpMaxHistLength.Value = EFPHistComboBox.DefaultMaxHistLength;

      cbPathValidateMode.Items.Add("[ Default ]");
      cbPathValidateMode.Items.AddRange(Enum.GetNames(typeof(TestPathMode)));
      efpPathValidateMode = new EFPListComboBox(efpForm, cbPathValidateMode);
      efpPathValidateMode.SelectedIndex = 0;

      cbMode.Items.AddRange(Enum.GetNames(typeof(FileDialogMode)));
      efpMode = new EFPListComboBox(efpForm, cbMode);
      efpMode.SelectedIndex = 0;

      efpDescription = new EFPTextBox(efpForm, edDescription);
      efpDescription.CanBeEmpty = true;

      efpShowSubFoldersButton = new EFPCheckBox(efpForm, cbShowSubFoldersButton);
      efpShowSubFoldersButton.EnabledEx = efpDialog[0].CheckedEx;

      efpDefaultPath = new EFPTextBox(efpForm, edDefaultPath);
      efpDefaultPath.CanBeEmpty = true;

      efpSetSelectedPath = new EFPCheckBox(efpForm, cbSetSelectedPath);
      efpSelectedPath = new EFPTextBox(efpForm, edSelectedPath);
      efpSelectedPath.CanBeEmpty = true;
      efpSelectedPath.ReadOnlyEx = new DepNot(efpSetSelectedPath.CheckedEx);

      efpFilter = new EFPTextBox(efpForm, edFilter);
      efpFilter.CanBeEmpty = true;
      efpFilter.EnabledEx = efpDialog[1].CheckedEx;

      btnOk.Image = EFPApp.MainImages.Images["Ok"];
      btnOk.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpOk = new EFPButton(efpForm, btnOk);
      efpOk.Click += EfpOk_Click;
    }

    #endregion

    #region Поля

    EFPRadioButtons efpDialog;
    EFPTextBox efpHistList;
    EFPTextBox efpConfigSectionName;
    EFPIntEditBox efpMaxHistLength;
    EFPListComboBox efpPathValidateMode;
    EFPListComboBox efpMode;
    EFPTextBox efpDescription;
    EFPCheckBox efpShowSubFoldersButton;
    EFPTextBox efpDefaultPath;
    EFPCheckBox efpSetSelectedPath;
    EFPTextBox efpSelectedPath;
    EFPTextBox efpFilter;

    #endregion

    #region Выполнение теста

    private void EfpOk_Click(object sender, EventArgs args)
    {
      HistFileSystemBrowserDialogBase dlg;
      if (efpDialog.SelectedIndex == 0)
      {
        dlg = new HistFolderBrowserDialog();
        ((HistFolderBrowserDialog)dlg).ShowSubFoldersButton = efpShowSubFoldersButton.Checked;
      }
      else
      {
        dlg = new HistFileBrowserDialog();
        ((HistFileBrowserDialog)dlg).Filter = efpFilter.Text;
      }

      dlg.HistList = new HistoryList(efpHistList.Control.Lines);
      dlg.MaxHistLength = efpMaxHistLength.Value;
      dlg.ConfigSectionName = efpConfigSectionName.Text;
      dlg.Mode = DataTools.GetEnum<FileDialogMode>(efpMode.SelectedItemString);
      if (efpPathValidateMode.SelectedIndex > 0)
        dlg.PathValidateMode = DataTools.GetEnum<TestPathMode>(efpPathValidateMode.SelectedItemString);
      dlg.Description = efpDescription.Text;
      dlg.DefaultPath = efpDefaultPath.Text;
      if (efpSetSelectedPath.Checked)
        dlg.SelectedPath = efpSelectedPath.Text;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      efpHistList.Control.Lines = dlg.HistList.ToArray();
      efpSetSelectedPath.Checked = false;
      efpSelectedPath.Text = dlg.SelectedPath;
    }

    #endregion
  }
}
