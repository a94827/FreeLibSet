﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.Models.Tree;

namespace TestTreeViews
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpModel = new EFPRadioButtons(efpForm, rbSimpleFileModel);

      efpDir = new EFPTextBox(efpForm, edDir);
      efpDir.CanBeEmpty = false;
      efpDir.Text = FileTools.ApplicationBaseDir.SlashedPath;
      EFPFolderBrowserButton efpBrowseDir = new EFPFolderBrowserButton(efpDir, btnBrowseDir);
      efpBrowseDir.ShowNewFolderButton = false;
      efpBrowseDir.PathValidateMode = FreeLibSet.IO.TestPathMode.DirectoryExists;
      efpDir.EnabledEx = efpModel[0].CheckedEx;

      efpCheckBoxes = new EFPCheckBox(efpForm, cbCheckBoxes);

      EFPButton efpOk = new EFPButton(efpForm, btnOk);
      efpOk.Click += EfpOk_Click;



    }

    #endregion

    #region Поля

    public EFPRadioButtons efpModel;

    public EFPTextBox efpDir;

    public EFPCheckBox efpCheckBoxes;

    #endregion

    #region Запуск теста

    private void EfpOk_Click(object sender, EventArgs args)
    {
      ITreeModel model;
      switch (efpModel.SelectedIndex)
      {
        case 0:
          model = new SimpleFileModel(new AbsPath(efpDir.Text));
          break;
        case 1:
          model = new ListTreeModel();
          break;
        default:
          throw new BugException();
      }

      TestCheckBoxesForm form = new TestCheckBoxesForm(model, efpCheckBoxes.Checked);
      EFPApp.ShowDialog(form, true);
    }

    #endregion
  }


}
