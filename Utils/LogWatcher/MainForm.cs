using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;

namespace LogWatcher
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();
      WinFormsTools.InitAppIcon(this);

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpForm.ConfigSectionName = "MainForm";

      efpGr1 = new EFPDataGridView(efpForm, gr1);
      efpGr1.Control.AutoGenerateColumns = false;
      efpGr1.Columns.AddImage();
      efpGr1.Columns.AddTextFill("Path", true, "Путь", 100, 10);
      efpGr1.Columns.AddDateTime("LastTime", true, "Время");
      efpGr1.Columns.AddInt("NewCount", true, "Новых", 3);
      efpGr1.Columns.AddInt("OldCount", true, "Старых", 3);
      efpGr1.DisableOrdering();
      efpGr1.GetCellAttributes += EfpGr1_GetCellAttributes;
      efpGr1.Control.MultiSelect = true;
      efpGr1.ReadOnly = true;
      efpGr1.Control.ReadOnly = true;
      efpGr1.CanView = true;
      efpGr1.CanMultiEdit = false;
      efpGr1.EditData += EfpGr1_EditData;
      efpGr1.RefreshData += EfpGr1_RefreshData;
      efpGr1.SelectedRowsMode = EFPDataGridViewSelectedRowsMode.RowIndex; // список не меняется в процессе работы

      EFPCommandItem ciSetOk = new EFPCommandItem("Edit", "SetOK");
      ciSetOk.MenuText = "Пометить как просмотренное";
      ciSetOk.ImageKey = "Ok";
      ciSetOk.ShortCut = Keys.F4;
      ciSetOk.Click += CiSetOk_Click;
      ciSetOk.GroupBegin = true;
      ciSetOk.GroupEnd = true;
      efpGr1.CommandItems.Add(ciSetOk);


      EFPCommandItem ciAbout = EFPAppCommandItems.CreateStdCommand(EFPAppStdCommandItems.About);
      ciAbout.Click += Program.CiAbout_Click;
      efpGr1.CommandItems.Add(ciAbout);

      EFPCommandItem ciExit = EFPAppCommandItems.CreateStdCommand(EFPAppStdCommandItems.Exit);
      ciExit.Click += Program.CiExit_Click;
      efpGr1.CommandItems.Add(ciExit);


      efpGr1.ToolBarPanel = panSpb1;
    }

    protected override void OnFormClosing(FormClosingEventArgs args)
    {
      if (args.CloseReason == CloseReason.UserClosing)
      {
        Hide();
        args.Cancel = true;
        return;
      }

      base.OnFormClosing(args);
    }

    #endregion

    #region Табличный просмотр

    EFPDataGridView efpGr1;

    private void EfpGr1_RefreshData(object sender, EventArgs args)
    {
      Program.TheWatcher.Update();
    }

    internal void SetData(DataTable tbl)
    {
      EFPDataGridViewSelection oldSel = efpGr1.Selection;
      efpGr1.Control.DataSource = tbl.DefaultView;
      efpGr1.Selection = oldSel;
    }

    private void EfpGr1_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (args.ColumnIndex == 0)
      {
        if (!args.DataRow.IsNull("IOError"))
        {
          args.Value = EFPApp.MainImages.Images["Error"];
          args.ToolTipText = "Ошибка получения списка файлов." + Environment.NewLine + args.DataRow["IOError"].ToString();
        }
        else if (DataTools.GetBool(args.DataRow, "NoDir"))
        {
          args.Value = EFPApp.MainImages.Images["Warning"];
          args.ToolTipText = "Каталог не найден";
        }
        else
        {
          string imageKey;
          if (DataTools.GetInt(args.DataRow, "NewCount") > 0)
          {
            ErrorMessageKind kind = (ErrorMessageKind)DataTools.GetInt(args.DataRow, "Kind");
            imageKey = EFPApp.GetErrorImageKey(kind);
          }
          else
            imageKey = "Ok";
          args.Value = EFPApp.MainImages.Images[imageKey];
        }
      }
    }

    private void EfpGr1_EditData(object sender, EventArgs args)
    {
      if (!efpGr1.CheckSingleRow())
        return;
      string dir = DataTools.GetString(efpGr1.CurrentDataRow, "Path");
      EFPApp.ShowWindowsExplorer(new AbsPath(dir));
    }

    private void CiSetOk_Click(object sender, EventArgs e)
    {
      foreach (DataRow row in efpGr1.SelectedDataRows)
      {
        row["RegTime"] = row["LastTime"];
        DataTools.IncInt(row, "OldCount", DataTools.GetInt(row, "NewCount"));
        row["NewCount"] = DBNull.Value;
      }

      efpGr1.InvalidateSelectedRows();
      Program.TheWatcher.WriteRegDates(efpGr1.SourceAsDataTable);
    }

    #endregion
  }
}
