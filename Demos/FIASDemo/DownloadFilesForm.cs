using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.FIAS;
using AgeyevAV.IO;
using AgeyevAV;

namespace FIASDemo
{
  /// <summary>
  /// ������ ���������� ���������� ������� ����������
  /// </summary>
  public partial class DownloadFilesForm : Form
  {
    #region ����������� �����

    public DownloadFilesForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpDir = new EFPTextBox(efpForm, edDir);
      efpDir.CanBeEmpty = false;
      EFPFolderBrowserButton efpBrowse = new EFPFolderBrowserButton(efpDir, btnBrowse);
      efpBrowse.ShowNewFolderButton = true;

      efpActualDate = new EFPDateBox(efpForm, edActualDate);
      efpActualDate.CanBeEmpty = false;

      efpFormat = new EFPListComboBox(efpForm, cbFormat);
    }

    #endregion

    #region ����

    EFPTextBox efpDir;

    EFPDateBox efpActualDate;

    EFPListComboBox efpFormat;

    #endregion

    #region ����������� ����� �������

    public static void PerformDownload()
    {
      DownloadFilesForm form = new DownloadFilesForm();
      form.cbFormat.Items.Add("XML");
      form.cbFormat.Items.Add("DBF");
      form.efpFormat.SelectedIndex = 0;

      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      FiasWebLoader loader = new FiasWebLoader(new AbsPath(form.efpDir.Text), form.efpActualDate.Value.Value);

      switch (form.efpFormat.SelectedIndex)
      {
        case 0: loader.Format = FiasDBUpdateSource.Xml; break;
        case 1: loader.Format = FiasDBUpdateSource.Dbf; break;
        default:
          throw new BugException("����������� ������");
      }

      using (Splash splash = new Splash("�������� ����������"))
      {
        loader.Splash = splash;
        loader.DownloadFiles();
      }
    }

    #endregion
  }
}