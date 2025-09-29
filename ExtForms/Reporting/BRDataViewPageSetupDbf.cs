using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.DBF;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupDbf : Form
  {
    #region Конструктор формы

    public BRDataViewPageSetupDbf(SettingsDialog dialog, IEFPDataView controlProvider)
    {
      InitializeComponent();
      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      #region Столбцы

      tblColumns = new DataTable();
      tblColumns.Columns.Add("Name", typeof(string));
      tblColumns.Columns.Add("Exported", typeof(bool));
      tblColumns.Columns.Add("DisplayName", typeof(string));
      tblColumns.Columns.Add("DbfName", typeof(string));
      tblColumns.Columns.Add("OrgDbfName", typeof(string));
      tblColumns.Columns.Add("Format", typeof(string));
      tblColumns.Columns.Add("ErrorDbfName", typeof(bool));
      tblColumns.Columns.Add("RepeatedDbfName", typeof(bool));
      lstColumns = new List<IEFPDataViewColumn>();
      foreach (IEFPDataViewColumn column in controlProvider.VisibleColumns)
      {
        if (column.DbfPreliminaryInfo == null)
          continue;
        tblColumns.Rows.Add(column.Name, DBNull.Value, column.DisplayName);
        lstColumns.Add(column);
      }

      ghColumns = new EFPDataGridView(page.BaseProvider, grColumns);
      ghColumns.Control.AutoGenerateColumns = false;
      ghColumns.Columns.AddCheckBox("Exported", true, "");
      ghColumns.Columns.LastAdded.DisplayName = Res.BRDataViewPageSetupDbf_Name_Exported;
      ghColumns.Columns.AddTextFill("DisplayName", true, Res.BRDataViewPageSetupDbf_ColTitle_DisplayName, 100, 10);
      ghColumns.Columns.LastAdded.CanIncSearch = true;
      ghColumns.Columns.AddText("DbfName", true, Res.BRDataViewPageSetupDbf_ColTitle_DbfName, 10, 10);
      ghColumns.Columns.AddText("Format", true, Res.BRDataViewPageSetupDbf_ColTitle_Format, 5, 2);
      ghColumns.MarkRowsColumnName = "Exported";
      ghColumns.ReadOnly = true;
      ghColumns.CanView = false;
      ghColumns.Control.ReadOnly = false;
      ghColumns.SetColumnsReadOnly(true);
      ghColumns.Columns["Exported"].GridColumn.ReadOnly = false;
      ghColumns.Columns["DbfName"].GridColumn.ReadOnly = false;
      ghColumns.CommandItems.OutHandler.Items.Clear(); // избегаем вложенного показа
      ghColumns.RowInfoNeeded += GhColumns_RowInfoNeeded;
      ghColumns.CellInfoNeeded += GhColumns_CellInfoNeeded;
      ghColumns.Validating += GhColumns_Validating;
      ghColumns.CellFinished += GhColumns_CellFinished;
      ghColumns.ToolBarPanel = panSpbColumns;

      ghColumns.Control.DataSource = tblColumns;

      #endregion

      #region Кодировка

      EncodingInfo[] encs = Encoding.GetEncodings();
      Array.Sort<EncodingInfo>(encs, BRDataViewPageSetupText.EncodingSortComparision);
      List<string> encNames = new List<string>(encs.Length);
      List<string> encCodes = new List<string>(encs.Length);
      foreach (EncodingInfo ei in encs)
      {
        if (EnvironmentTools.IsMono)
          encNames.Add(ei.Name);
        else
          encNames.Add(ei.DisplayName); // В mono выводится странный текст "Globalization.cpXXX"
        encCodes.Add(StdConvert.ToString(ei.CodePage));
      }
      cbDbfCodePage.Items.AddRange(encNames.ToArray());
      efpDbfCodePage = new EFPListComboBox(page.BaseProvider, cbDbfCodePage);
      efpDbfCodePage.Codes = encCodes.ToArray();

      #endregion

      page.Text = Res.BRDataViewPageSetupDbf_Title_Tab;
      page.ToolTipText = Res.BRDataViewPageSetupDbf_ToolTip_Tab;
      page.ImageKey = "Settings";

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    private IEFPDataView _ControlProvider;
    private BRDataViewSettingsDataItem _ViewData;

    #endregion

    #region Поля

    private List<IEFPDataViewColumn> lstColumns;
    private EFPDataGridView ghColumns;

    DataTable tblColumns;

    private EFPListComboBox efpDbfCodePage;

    #endregion

    #region Проверка столбцов

    private void GhColumns_RowInfoNeeded(object sender, EFPDataGridViewRowInfoEventArgs args)
    {
      if (args.DataRow == null)
        return;

      if (DataTools.GetBoolean(args.DataRow, "ErrorDbfName"))
        args.AddRowError(Res.BRDataViewPageSetupDbf_Err_BadFieldName, "DbfName");
      if (DataTools.GetBoolean(args.DataRow, "RepeatedDbfName"))
        args.AddRowError(Res.BRDataViewPageSetupDbf_Err_RepeatedFieldName, "DbfName");
    }

    private void GhColumns_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      if (args.DataRow == null)
        return;
      switch (args.ColumnName)
      {
        case "DbfName":
        case "Format":
          if (!DataTools.GetBoolean(args.DataRow, "Exported"))
            args.Grayed = true;
          break;
      }
    }

    private void GhColumns_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      foreach (DataRow row in tblColumns.Rows)
      {
        row["ErrorDbfName"] = false;
        row["RepeatedDbfName"] = false;
      }

      TypedStringDictionary<DataRow> dict = new TypedStringDictionary<DataRow>(true);
      for (int i = 0; i < lstColumns.Count; i++)
      {
        DataRow row = tblColumns.Rows[i];
        if (!DataTools.GetBoolean(row, "Exported"))
          continue;

        string nm = row["DbfName"].ToString();
        if (!DbfFieldInfo.IsValidFieldName(nm))
        {
          row["ErrorDbfName"] = true;
          args.SetError(Res.BRDataViewPageSetupDbf_Err_BadFieldName);
        }
        else if (dict.ContainsKey(nm))
        {
          row["RepeatedDbfName"] = true;
          dict[nm]["RepeatedDbfName"] = true;
          args.SetError(String.Format(Res.BRDataViewPageSetupDbf_Err_RepeatedFieldName2, nm));
        }
        else
          dict.Add(nm, row);
      }

      if (dict.Count == 0)
        args.SetError(Res.BRDataViewPageSetupDbf_Err_NoFields);
    }

    private void GhColumns_CellFinished(object sender, EFPDataGridViewCellFinishedEventArgs args)
    {
      Validate();
      ghColumns.Control.Invalidate();
    }

    #endregion

    #region Чтение и запись значений

    private void Page_DataToControls(object sender, EventArgs args)
    {
      string[] names = _ViewData.GetRealDbfFieldNames(lstColumns.ToArray());
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < lstColumns.Count; i++)
      {
        DataRow row = tblColumns.Rows[i];
        row["Exported"] = _ViewData.GetDbfExported(lstColumns[i]);
        row["DbfName"] = names[i];
        row["OrgDbfName"] = names[i];

        DbfFieldTypePreliminaryInfo pi;
        if (lstColumns[i].DbfInfo.IsEmpty)
          pi = lstColumns[i].DbfPreliminaryInfo;
        else
          pi = new DbfFieldTypePreliminaryInfo(lstColumns[i].DbfInfo);
        if (pi.Type == ' ')
          row["Format"] = "?";
        else
          row["Format"] = pi.ToString();
      }

      efpDbfCodePage.SelectedCode = StdConvert.ToString(_ViewData.DbfCodePage);
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      for (int i = 0; i < lstColumns.Count; i++)
      {
        DataRow row = tblColumns.Rows[i];
        _ViewData.SetDbfExported(lstColumns[i], DataTools.GetBoolean(row, "Exported"));
        string newName = row["DbfName"].ToString();
        string orgName = row["OrgDbfName"].ToString();
        if (newName != orgName)
          _ViewData.SetDbfFieldName(lstColumns[i], newName);
      }
      ghColumns.Validate();
      ghColumns.Control.Invalidate();

      _ViewData.DbfCodePage = StdConvert.ToInt32(efpDbfCodePage.SelectedCode);
    }

    #endregion
  }

  internal class BRDataViewFileDbf
  {
    #region Конструктор

    public BRDataViewFileDbf(BRDataViewSettingsDataItem settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");
      _Settings = settings;
    }

    private readonly BRDataViewSettingsDataItem _Settings;

    #endregion

    public void CreateFile(IEFPDataView controlProvider, FreeLibSet.IO.AbsPath filePath)
    {
      IEFPDataViewColumn[] columns = controlProvider.VisibleColumns;
      string[] names = _Settings.GetRealDbfFieldNames(columns);
      GetExportedColumns(ref columns, ref names);

      Encoding enc = Encoding.GetEncoding(_Settings.DbfCodePage);
      if (enc == null)
        throw new BugException("Unknown encoding " + _Settings.DbfCodePage.ToString());
      DbfStruct dbs = new DbfStruct();
      for (int i = 0; i < columns.Length; i++)
        dbs.Add(GetDbfFieldInfo(columns[i], names[i], DbfFileFormat.dBase3, enc));

      using (DbfFile dbf = new DbfFile(filePath, dbs, enc, DbfFileFormat.dBase3))
      {
        // Цикл foreach нельзя использовать для множества значений
        System.Collections.IEnumerator[] ens = new System.Collections.IEnumerator[columns.Length];
        for (int i = 0; i < columns.Length; i++)
          ens[i] = columns[i].ValueEnumerable.GetEnumerator();
        while (ens[0].MoveNext())
        {
          for (int i = 1; i < columns.Length; i++)
            ens[i].MoveNext();

          dbf.AppendRecord();
          for (int i = 0; i < columns.Length; i++)
            dbf.SetValue(i, ens[i].Current);
        }

        // Метод IEnumerator.Dispose() можно не вызывать, т.к. это точно известные перечислители

        dbf.Flush();
      }
    }

    /// <summary>
    /// Оставляет в списках только экспортируемые столбцы
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="names"></param>
    private void GetExportedColumns(ref IEFPDataViewColumn[] columns, ref string[] names)
    {
      List<IEFPDataViewColumn> lstCols = new List<IEFPDataViewColumn>();
      List<string> lstNames = new List<string>();
      for (int i = 0; i < columns.Length; i++)
      {
        if (_Settings.GetDbfExported(columns[i]))
        {
          lstCols.Add(columns[i]);
          lstNames.Add(names[i]);
        }
      }
      columns = lstCols.ToArray();
      names = lstNames.ToArray();
      if (columns.Length == 0)
        throw new BugException("There are no exported columns");
    }

    private DbfFieldInfo GetDbfFieldInfo(IEFPDataViewColumn column, string colName, DbfFileFormat format, Encoding enc)
    {
      if (column.DbfPreliminaryInfo == null)
        throw new BugException("Column \"" + column.Name + "\" cannot be exported to DBF-file");

      if (!column.DbfInfo.IsEmpty)
      {
        if (column.DbfInfo.Type == 'C' && (!enc.IsSingleByte))
        {
          DbfFieldTypePreliminaryInfo pi = new DbfFieldTypePreliminaryInfo();
          pi.Type = 'C';
          pi.Length = column.DbfInfo.Length;
          pi.LengthIsDefined = false;
          DbfFieldTypeDetector dtr = new DbfFieldTypeDetector();
          dtr.PreliminaryInfo = pi;
          dtr.Format = format;
          dtr.Encoding = enc;
          dtr.UseMemo = false;
          dtr.ColumnName = colName;
          foreach (object value in column.ValueEnumerable)
            dtr.ApplyValue(value);
          return dtr.Result;
        }
        else
          return new DbfFieldInfo(colName, column.DbfInfo);
      }
      else
      {
        DbfFieldTypeDetector dtr = new DbfFieldTypeDetector();
        dtr.PreliminaryInfo = column.DbfPreliminaryInfo;
        dtr.Format = format;
        dtr.Encoding = enc;
        dtr.UseMemo = true;
        dtr.ColumnName = colName;
        if (!dtr.IsCompleted)
        {
          foreach (object value in column.ValueEnumerable)
          {
            dtr.ApplyValue(value);
            if (dtr.IsCompleted)
              break;
          }
        }
        return dtr.Result;
      }
    }
  }
}

