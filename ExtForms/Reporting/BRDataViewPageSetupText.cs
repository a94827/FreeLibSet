using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.Models.Tree;
using FreeLibSet.Text;

namespace FreeLibSet.Forms.Reporting
{
  internal partial class BRDataViewPageSetupText : Form
  {
    #region Конструктор формы

    public BRDataViewPageSetupText(SettingsDialog dialog, IEFPDataView controlProvider, bool isCsv)
    {
      InitializeComponent();
      if (isCsv)
        grpMain.Text = Res.BRDataViewPageSetupText_Title_CSV;

      _ControlProvider = controlProvider;
      _ViewData = dialog.Data.GetRequired<BRDataViewSettingsDataItem>();
      _IsCsv = isCsv;

      SettingsDialogPage page = dialog.Pages.Add(MainPanel);

      EncodingInfo[] encs = Encoding.GetEncodings();
      Array.Sort<EncodingInfo>(encs, EncodingSortComparision);
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
      cbCodePage.Items.AddRange(encNames.ToArray());
      efpCodePage = new EFPListComboBox(page.BaseProvider, cbCodePage);
      efpCodePage.Codes = encCodes.ToArray();

      efpFieldDelimiter = new EFPTextComboBox(page.BaseProvider, cbFieldDelimiter);
      efpFieldDelimiter.Validating += EfpFieldDelimiter_Validating;
      efpQuote = new EFPTextComboBox(page.BaseProvider, cbQuote);

      efpSingleLineField = new EFPCheckBox(page.BaseProvider, cbSingleLineField);
      efpRemoveDoubleSpaces = new EFPCheckBox(page.BaseProvider, cbRemoveDoubleSpaces);
      efpExpColumnHeaders = new EFPCheckBox(page.BaseProvider, cbExpColumnHeaders);
      efpExpColumnHeaders.Enabled = _ViewData.UseExpColumnHeaders;

      if (!_IsCsv)
      {
        efpFieldDelimiter.Visible = false;
        efpQuote.Visible = false;
        efpSingleLineField.Checked = true;
        efpSingleLineField.Enabled = false;
      }

      page.Text = Res.BRDataViewPageSetupText_Title_Tab;
      page.ToolTipText = Res.BRDataViewPageSetupText_ToolTip_Tab;
      page.ImageKey = "Settings";

      page.DataToControls += Page_DataToControls;
      page.DataFromControls += Page_DataFromControls;
    }

    internal static int EncodingSortComparision(EncodingInfo x, EncodingInfo y)
    {
      if (EnvironmentTools.IsMono)
        return String.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
      else
        return String.Compare(x.DisplayName, y.DisplayName, StringComparison.CurrentCultureIgnoreCase);
    }

    private IEFPDataView _ControlProvider;
    private BRDataViewSettingsDataItem _ViewData;
    private bool _IsCsv;

    #endregion

    #region Поля

    private EFPListComboBox efpCodePage;
    private EFPTextComboBox efpFieldDelimiter, efpQuote;
    private EFPCheckBox efpSingleLineField, efpRemoveDoubleSpaces, efpExpColumnHeaders;

    private void EfpFieldDelimiter_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      if (efpFieldDelimiter.Text.Length != 1)
        args.SetError(Res.BRDataViewPageSetupText_Err_SingleCharRequired);
    }

    private void EfpQuote_Validating(object sender, UICore.UIValidatingEventArgs args)
    {
      if (efpQuote.Text.Length != 1)
        args.SetError(Res.BRDataViewPageSetupText_Err_SingleCharRequired);
    }

    #endregion

    #region Чтение и запись значений

    private void Page_DataToControls(object sender, EventArgs args)
    {
      efpCodePage.SelectedCode = StdConvert.ToString(_ViewData.CodePage);
      if (_IsCsv)
      {
        efpFieldDelimiter.Text = new string(_ViewData.FieldDelimiter, 1);
        efpQuote.Text = new string(_ViewData.Quote, 1);
        efpSingleLineField.Checked = _ViewData.SingleLineField;
      }
      efpRemoveDoubleSpaces.Checked = _ViewData.RemoveDoubleSpaces;
      if (_ViewData.UseExpColumnHeaders)
        efpExpColumnHeaders.Checked = _ViewData.ExpColumnHeaders;
    }

    private void Page_DataFromControls(object sender, EventArgs args)
    {
      _ViewData.CodePage = StdConvert.ToInt32(efpCodePage.SelectedCode);
      if (_IsCsv)
      {
        _ViewData.FieldDelimiter = efpFieldDelimiter.Text[0];
        _ViewData.Quote = efpQuote.Text[0];
        _ViewData.SingleLineField = efpSingleLineField.Checked;
      }
      _ViewData.RemoveDoubleSpaces = efpRemoveDoubleSpaces.Checked;
      if (_ViewData.UseExpColumnHeaders)
        _ViewData.ExpColumnHeaders = efpExpColumnHeaders.Checked;
    }

    #endregion
  }

  internal class BRDataViewFileText
  {
    #region Конструктор

    public BRDataViewFileText(BRDataViewSettingsDataItem settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");
      _Settings = settings;
    }

    private readonly BRDataViewSettingsDataItem _Settings;

    #endregion

    #region Общие методы

    private StreamWriter CreateStreamWriter(AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");
      FreeLibSet.IO.FileTools.ForceDirs(filePath.ParentDir);
      Encoding enc = Encoding.GetEncoding(_Settings.CodePage);
      StreamWriter sw = new StreamWriter(filePath.Path, false, enc);
      return sw;
    }

    private ITextConvert CreateTextConvert(bool isCsv)
    {
      if (isCsv)
      {
        CsvTextConvert conv = new CsvTextConvert();
        conv.FieldDelimiter = _Settings.FieldDelimiter;
        conv.Quote = _Settings.Quote;
        return conv;
      }
      else
      {
        TabTextConvert conv = new TabTextConvert();
        return conv;
      }
    }

    private string PrepareValue(string value, bool isCsv)
    {
      if (String.IsNullOrEmpty(value))
        return String.Empty;

      value = value.Replace('\t', ' ');
      if ((!isCsv) || _Settings.SingleLineField)
      {
        for (int i = 0; i < DataTools.AllPossibleLineSeparators.Length; i++)
          value = value.Replace(DataTools.AllPossibleLineSeparators[i], " ");
      }
      if (_Settings.RemoveDoubleSpaces)
      {
        value = DataTools.RemoveDoubleChars(value, ' ');
        value = value.Trim();
      }
      return value;
    }

    #endregion

    public void CreateFile(IEFPDataView controlProvider, FreeLibSet.IO.AbsPath filePath, bool isCsv)
    {
      if (controlProvider is EFPDataGridView)
        CreateFileDataGridView((EFPDataGridView)controlProvider, filePath, isCsv);
      else if (controlProvider is EFPDataTreeView)
        CreateFileDataTreeView((EFPDataTreeView)controlProvider, filePath, isCsv);
      else
        throw new BugException();
    }

    #region EFPDataGridView 

    private void CreateFileDataGridView(EFPDataGridView controlProvider, FreeLibSet.IO.AbsPath filePath, bool isCsv)
    {
      ITextConvert conv = CreateTextConvert(isCsv);
      List<string> lst = new List<string>(controlProvider.Columns.Count);
      StringBuilder sb = new StringBuilder();

      EFPDataGridViewColumn[] cols = controlProvider.VisibleColumns; // в порядке вывода на экран
      using (StreamWriter sw = CreateStreamWriter(filePath))
      {
        if (_Settings.ExpColumnHeaders && controlProvider.Control.ColumnHeadersVisible)
        {
          // Не используем печатные заголовки PrintHeaders, так как в файле должна быть только одна строка заголовков
          lst.Clear();
          for (int j = 0; j < cols.Length; j++)
          {
            if (cols[j].Printable)
              lst.Add(cols[j].GridColumn.HeaderText);
          }
          sb.Length = 0;
          conv.ToString(sb, lst.ToArray());
          sw.WriteLine(sb);
        }

        for (int i = 0; i < controlProvider.Control.RowCount; i++)
        {
          EFPDataGridViewRowAttributesEventArgs rowArgs = controlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
          lst.Clear();
          for (int j = 0; j < cols.Length; j++)
          {
            if (cols[j].Printable)
            {
              EFPDataGridViewCellAttributesEventArgs cellArgs = controlProvider.DoGetCellAttributes(cols[j].Index);
              string value;
              if (cellArgs.ContentVisible)
              {
                object v = cellArgs.FormattedValue;
                if (v is Boolean)
                  v = _Settings.GetBoolValue((bool)v);
                value = DataTools.GetString(v);
              }
              else
                value = String.Empty;
              lst.Add(PrepareValue(value, isCsv));
            }
          }

          sb.Length = 0;
          conv.ToString(sb, lst.ToArray());
          sw.WriteLine(sb);
        }
      }
    }

    #endregion

    #region EFPDataTreeView 

    private void CreateFileDataTreeView(EFPDataTreeView controlProvider, FreeLibSet.IO.AbsPath filePath, bool isCsv)
    {
      ITextConvert conv = CreateTextConvert(isCsv);
      List<string> lst = new List<string>(controlProvider.Columns.Count);
      StringBuilder sb = new StringBuilder();

      //EFPDataTreeViewColumn[] cols = controlProvider.VisibleColumns; // в порядке вывода на экран
      InteractiveControl[] ctrls = controlProvider.GetNodeControls<InteractiveControl>();
      using (StreamWriter sw = CreateStreamWriter(filePath))
      {
        if (_Settings.ExpColumnHeaders && controlProvider.Control.UseColumns)
        {
          // Не используем печатные заголовки PrintHeaders, так как в файле должна быть только одна строка заголовков
          lst.Clear();
          for (int j = 0; j < ctrls.Length; j++)
          {
            lst.Add(ctrls[j].ParentColumn.Header);
          }
          sb.Length = 0;
          conv.ToString(sb, lst.ToArray());
          sw.WriteLine(sb);
        }


        foreach (TreePath path in new TreePathEnumerable(controlProvider.Control.Model))
        {
          TreeNodeAdv node = controlProvider.Control.FindNode(path, true);
          lst.Clear();
          for (int j = 0; j < ctrls.Length; j++)
          {
            InteractiveControl nc = ctrls[j];
            object v = nc.GetValue(node);
            if (v is Boolean)
              v = _Settings.GetBoolValue((bool)v);
            string s = null;
            if (v is IFormattable)
            {
              BaseFormattedTextControl ctlFormat = nc as BaseFormattedTextControl;
              if (ctlFormat != null)
                s = ((IFormattable)v).ToString(ctlFormat.Format, ctlFormat.FormatProvider);
            }
            if (s == null)
              s = DataTools.GetString(v);

            lst.Add(PrepareValue(s, isCsv));
          }

          sb.Length = 0;
          conv.ToString(sb, lst.ToArray());
          sw.WriteLine(sb);
        }
      }
    }

    #endregion
  }
}
