using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.ExtDB;
using AgeyevAV.ExtForms.FIAS;
using System.Data.Common;
using AgeyevAV.IO;
using AgeyevAV;

namespace FIASDemo
{
  public partial class CreateDBForm : Form
  {
    #region Конструктор формы

    public CreateDBForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("Insert");

      EFPFormProvider efpForm=new EFPFormProvider(this);
      TheTabControl.ImageList = EFPApp.MainImages;

      #region База данных

      tpDatabase.ImageKey = "Database";

      efpName = new EFPTextBox(efpForm, edName);
      efpName.CanBeEmpty = false;

      cbProvider.Items.AddRange(new string[]{DBxProviderNames.SQLite, DBxProviderNames.Npgsql, DBxProviderNames.Sql});
      efpProvider = new EFPListComboBox(efpForm, cbProvider);
      efpProvider.CanBeEmpty = false;

      efpConnectionString = new EFPTextBox(efpForm, edConnectionString);
      efpConnectionString.CanBeEmpty = false;

      EFPButton efpBuildConnection = new EFPButton(efpForm, btnBuildConnection);
      efpBuildConnection.Click += new EventHandler(efpBuildConnection_Click);

      #endregion

      #region Настройки

      tpSettings.ImageKey = "Settings";

      efpDBSettings = new EFPFiasDBSettingsPanel(efpForm, DBSettingsPanel);

      #endregion
    }

    #endregion

    #region База данных

    public EFPTextBox efpName;

    public EFPListComboBox efpProvider;

    public EFPTextBox efpConnectionString;

    void efpBuildConnection_Click(object sender, EventArgs args)
    {
      switch (efpProvider.SelectedItemString)
      { 
        case "":
          EFPApp.ShowTempMessage("Провайдер не выбран");
          break;
        case DBxProviderNames.SQLite:
          BuildSQLiteConnection();
          break;
        case DBxProviderNames.Sql:
          BuildSqlExpressConnection();
          break;
        default:
          EFPApp.ErrorMessageBox("Не реализовано для этого провайдера");
          break;
      }
    }

    private void BuildSQLiteConnection()
    {
      DbConnectionStringBuilder csb = DBxManager.Managers[DBxProviderNames.SQLite].ProviderFactory.CreateConnectionStringBuilder();
      try
      {
        csb.ConnectionString = efpConnectionString.Text;
      }
      catch { }
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Title = "База данных SQLite";
      object x;
      csb.TryGetValue("Data Source", out x);
      string s=DataTools.GetString(x);
      if (s.Length > 0)
        dlg.FileName = s;
      else
      {
        dlg.InitialDirectory = FileTools.ApplicationBaseDir.Path;
        if (!String.IsNullOrEmpty(efpName.Text))
          dlg.FileName = new AbsPath(FileTools.ApplicationBaseDir, efpName.Text + ".db").Path;
      }
      dlg.Filter = "Файлы базы данных|*.db";
      dlg.CheckFileExists = false;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      csb["data source"] = dlg.FileName;
      csb["page size"] = 65536; // максимальный размер страницы
      efpConnectionString.Text = csb.ConnectionString;
    }

    private void BuildSqlExpressConnection()
    {
      DbConnectionStringBuilder csb = DBxManager.Managers[DBxProviderNames.Sql].ProviderFactory.CreateConnectionStringBuilder();
      try
      {
        csb.ConnectionString = efpConnectionString.Text;
      }
      catch { }
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Title = "База данных mdf (MS SQL Server Express)";
      object x;
      csb.TryGetValue("AttachDbFilename", out x);
      string s = DataTools.GetString(x);
      if (s.Length > 0)
        dlg.FileName = s;
      else
      {
        dlg.InitialDirectory = FileTools.ApplicationBaseDir.Path;
        if (!String.IsNullOrEmpty(efpName.Text))
          dlg.FileName = new AbsPath(FileTools.ApplicationBaseDir, efpName.Text + ".mdf").Path;
      }
      dlg.Filter = "Файлы базы данных MS SQL|*.mdf";
      dlg.CheckFileExists = false;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      csb["Data Source"] = @".\SQLEXPRESS";
      csb["Integrated Security"] = true;
      csb["AttachDbFilename"] = dlg.FileName;
      efpConnectionString.Text = csb.ConnectionString;
    }

    #endregion

    #region Настройки

    public EFPFiasDBSettingsPanel efpDBSettings;

    #endregion
  }
}