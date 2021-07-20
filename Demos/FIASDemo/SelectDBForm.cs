using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtForms;
using AgeyevAV.Config;
using System.Data;
using AgeyevAV.FIAS;
using AgeyevAV;
using AgeyevAV.ExtForms.FIAS;

namespace FIASDemo
{
  public class SelectDBForm : OKCancelGridForm
  {
    #region Конструктор

    public SelectDBForm()
    {
      Text = "Выбор базы данных";
      efpGrid = new EFPDataGridView(base.ControlWithToolBar);
      efpGrid.Control.AutoGenerateColumns = false;
      efpGrid.Columns.AddTextFill("Name", true, "Условное имя базы данных", 100, 10);
      efpGrid.Columns.LastAdded.CanIncSearch = true;
      efpGrid.ReadOnly = false;
      efpGrid.Control.ReadOnly = true;
      efpGrid.CanView = false;
      efpGrid.EditData += new EventHandler(efpGrid_EditData);
      efpGrid.CommandItems.EnterAsOk = true;
      InfoLabel lbl = AddInfoLabel(System.Windows.Forms.DockStyle.Bottom);
      lbl.Text = "Демо-приложение позволяет создать несколько баз данных классификаторов разных форматов и с разными настройками." + Environment.NewLine +
        "Чтобы создать базу данных нажмите кнопку [+]. Или выберите базу данных из списка" + Environment.NewLine +
        "В реальном приложении обычно используется единственная база данных с предопределенными настройками";
      lbl.Icon = System.Windows.Forms.MessageBoxIcon.Information;
      lbl.IconSize = MessageBoxIconSize.Large;

      FormProvider.AddFormCheck(new EFPValidatingEventHandler(CheckForm));
    }

    public EFPDataGridView efpGrid;

    #endregion

    #region Загрузка и сохранение списка

    public CfgPart Cfg;

    public DataTable Table;

    private bool TableChanged;

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);

      Table = new DataTable();
      Table.Columns.Add("Name", typeof(string));
      Table.Columns.Add("ProviderName", typeof(string));
      Table.Columns.Add("ConnectionString", typeof(string));
      Table.Columns.Add("FiasDBSettings", typeof(string));

      CfgPart lst = Cfg.GetChild("Databases", true);
      string[] a = lst.GetChildNames();
      for (int i = 0; i < a.Length; i++)
      {
        CfgPart db = lst.GetChild(a[i], false);
        Table.Rows.Add(db.GetString("Name"), db.GetString("ProviderName"), db.GetString("ConnectionString"), db.GetString("FiasDBSettings"));
      }

      efpGrid.Control.DataSource = Table.DefaultView;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs args)
    {
      base.OnClosing(args);

      if (!TableChanged)
        return;


      CfgPart lst = Cfg.GetChild("Databases", true);
      lst.Clear();

      int cnt = 0;
      foreach (DataRow row in Table.Rows)
      {
        cnt++;
        CfgPart db = lst.GetChild("Item" + cnt.ToString(), true);
        db.SetString("Name", DataTools.GetString(row, "Name"));
        db.SetString("ProviderName", DataTools.GetString(row, "ProviderName"));
        db.SetString("ConnectionString", DataTools.GetString(row, "ConnectionString"));
        db.SetString("FiasDBSettings", DataTools.GetString(row, "FiasDBSettings"));
      }

      TableChanged = false;
    }

    private void CheckForm(object sender, EFPValidatingEventArgs args)
    {
      if (efpGrid.CurrentDataRow == null)
        args.SetError("База данных должна быть выбрана");
    }


    #endregion

    #region Редактирование списка

    void efpGrid_EditData(object sender, EventArgs args)
    {
      DataRow row;
      FiasDBSettings dbs;
      switch (efpGrid.State)
      {
        case EFPDataGridViewState.Insert:
          using (CreateDBForm form = new CreateDBForm())
          {
            if (EFPApp.ShowDialog(form, false) != System.Windows.Forms.DialogResult.OK)
              return;

            dbs = form.efpDBSettings.DBSettings;

            row = Table.Rows.Add(form.efpName.Text,
              form.efpProvider.SelectedItemString,
              form.efpConnectionString.Text,
              dbs.AsXmlText);
          }

          efpGrid.CurrentDataRow = row;
          TableChanged = true;

          FormProvider.CloseForm(System.Windows.Forms.DialogResult.OK);

          break;

        case EFPDataGridViewState.Edit:
        case EFPDataGridViewState.View:
          if (!efpGrid.CheckSingleRow())
            return;

          dbs = new FiasDBSettings();
          dbs.AsXmlText = DataTools.GetString(efpGrid.CurrentDataRow, "FiasDBSettings");
          EFPFiasDBSettingsPanel.ShowSettings(dbs);
          break;

        case EFPDataGridViewState.Delete:
          EFPApp.ErrorMessageBox("Не реализовано");
          break;
      }
    }

    #endregion
  }
}
