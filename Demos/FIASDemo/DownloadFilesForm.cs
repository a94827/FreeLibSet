using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.FIAS;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace FIASDemo
{
  /// <summary>
  /// Диалог параметров скачивания архивов обновлений
  /// </summary>
  public partial class DownloadFilesForm : Form
  {
    #region Конструктор формы

    public DownloadFilesForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpDir = new EFPTextBox(efpForm, edDir);
      efpDir.CanBeEmpty = false;
      EFPFolderBrowserButton efpBrowse = new EFPFolderBrowserButton(efpDir, btnBrowse);
      efpBrowse.ShowNewFolderButton = true;

      efpActualDate = new EFPDateTimeBox(efpForm, edActualDate);
      efpActualDate.CanBeEmpty = false;

      efpFormat = new EFPListComboBox(efpForm, cbFormat);
    }

    #endregion

    #region Поля

    EFPTextBox efpDir;

    EFPDateTimeBox efpActualDate;

    EFPListComboBox efpFormat;

    #endregion

    #region Статический метод запуска

    public static void PerformDownload()
    {
      DownloadFilesForm form = new DownloadFilesForm();
      form.cbFormat.Items.Add("XML");
      form.cbFormat.Items.Add("DBF");
      form.efpFormat.SelectedIndex = 0;

      if (EFPApp.ShowDialog(form, true) != DialogResult.OK)
        return;

      FiasWebLoader loader = new FiasWebLoader(new AbsPath(form.efpDir.Text), form.efpActualDate.Value);

      switch (form.efpFormat.SelectedIndex)
      {
        case 0: loader.Format = FiasDBUpdateSource.Xml; break;
        case 1: loader.Format = FiasDBUpdateSource.Dbf; break;
        default:
          throw new BugException("Неизвестный формат");
      }

      using (Splash splash = new Splash("Загрузка обновлений"))
      {
        loader.Splash = splash;
        loader.DownloadFiles();
      }
    }

    #endregion
  }
}