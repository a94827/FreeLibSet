using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.IO;

namespace NormTextFile
{
  public partial class MianForm : Form
  {
    #region Конструктор

    public MianForm()
    {
      InitializeComponent();
      Icon = WinFormsTools.AppIcon;
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpDir = new EFPHistComboBox(efpForm, cbDir);
      efpDir.CanBeEmpty = false;

      EFPFolderBrowserButton efpBrowse = new EFPFolderBrowserButton(efpDir, btnBrowse);
      efpBrowse.ShowNewFolderButton = false;

      efpNested = new EFPCheckBox(efpForm, cbNested);

      efpMask = new EFPHistComboBox(efpForm, cbMask);
      efpMask.CanBeEmpty = false;


      _Encodings = new List<EncodingInfo>(); // подходящие кодировки
      List<string> lstCodePages = new List<string>(); // Кодовые страницы как строки
      List<string> lstEncNames = new List<string>(); // для отображения в списке
      EncodingInfo[] aEI = Encoding.GetEncodings();
      Array.Sort<EncodingInfo>(aEI, CompareEncodingInfo);
      for (int i = 0; i < aEI.Length; i++)
      {
        _Encodings.Add(aEI[i]);
        lstCodePages.Add(StdConvert.ToString(aEI[i].CodePage));
        lstEncNames.Add(aEI[i].DisplayName + " (" + aEI[i].CodePage.ToString() + ")");
      }

      efpCodePage = new EFPListComboBox(efpForm, cbCodePage);
      efpCodePage.Control.Items.AddRange(lstEncNames.ToArray());
      efpCodePage.Codes = lstCodePages.ToArray();
      efpCodePage.CanBeEmpty = false;

      btnStart.Image = EFPApp.MainImages.Images["Ok"];
      btnStart.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpStart = new EFPButton(efpForm, btnStart);
      efpStart.Click += EfpStart_Click;

      efpRes = new EFPDataGridView(efpForm, grRes);
      efpRes.Control.AutoGenerateColumns = false;

      efpRes.Columns.AddTextFill("FileName", true, "Файл", 50, 10);
      efpRes.Columns.AddTextFill("Result", true, "Результат", 50, 10);
      efpRes.DisableOrdering();
      efpRes.Control.ReadOnly = true;
      efpRes.ReadOnly = true;
      efpRes.CanView = false;

      try
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo("Settings", EFPConfigCategories.UserParams), EFPConfigMode.Read, out cfg))
        {
          efpDir.HistList = cfg.GetHist("Directory");
          efpNested.Checked = cfg.GetBool("Nested");
          efpMask.HistList = cfg.GetHist("Mask");
          efpCodePage.SelectedCode = cfg.GetString("DefaultCodePage");
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка чтения настроек");
      }

      try
      {
        if (String.IsNullOrEmpty(efpCodePage.SelectedCode))
          efpCodePage.SelectedCode = StdConvert.ToString(Encoding.Default.CodePage);
      }
      catch { }
    }

    private int CompareEncodingInfo(EncodingInfo x, EncodingInfo y)
    {
      return String.Compare(x.DisplayName, y.DisplayName);
    }

    #endregion

    #region Поля

    EFPHistComboBox efpDir;

    EFPCheckBox efpNested;

    EFPHistComboBox efpMask;

    EFPDataGridView efpRes;

    List<EncodingInfo> _Encodings;

    EFPListComboBox efpCodePage;

    #endregion

    #region Start

    private void EfpStart_Click(object sender, EventArgs args)
    {
      #region Запись конфигурации

      try
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo("Settings", EFPConfigCategories.UserParams), EFPConfigMode.Write, out cfg))
        {
          cfg.SetHist("Directory", efpDir.HistList);
          cfg.SetBool("Nested", efpNested.Checked);
          cfg.SetHist("Mask", efpMask.HistList);
          cfg.SetString("DefaultCodePage", efpCodePage.SelectedCode);
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка записи настроек");
      }

      int defCodePage = StdConvert.ToInt32(efpCodePage.SelectedCode);
      Encoding defEnc = Encoding.GetEncoding(defCodePage);

      #endregion

      using (Splash spl = new Splash("Преобразование файлов"))
      {
        DataTable table = new DataTable();
        table.Columns.Add("FileName", typeof(string));
        table.Columns.Add("Result", typeof(string));

        try
        {
          AbsPath dir = new AbsPath(efpDir.Text);
          string[] aFiles = System.IO.Directory.GetFiles(dir.Path, efpMask.Text, efpNested.Checked ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly);
          spl.PercentMax = aFiles.Length;
          for (int i = 0; i < aFiles.Length; i++)
          {
            AbsPath file = new AbsPath(aFiles[i]);
            spl.PhaseText = file.FileName;

            string res = ProcessOneFile(file, defEnc);
            table.Rows.Add(file.FileName, res);

            spl.IncPercent();
          }
        }
        catch (UserCancelException)
        {
        }

        grRes.DataSource = table.DefaultView;
      }
    }

    #endregion

    #region Обработка файла

    private static string ProcessOneFile(AbsPath file, Encoding defEnc)
    {
      byte[] b1 = System.IO.File.ReadAllBytes(file.Path);

      Encoding enc;
      if (DataTools.ArrayStartsWith<byte>(b1, Encoding.UTF8.GetPreamble()))
        enc = Encoding.UTF8;
      else
        enc = defEnc;

      List<string> lines = new List<string>();
      using (MemoryStream ms1 = new MemoryStream(b1))
      {
        using (StreamReader rdr = new StreamReader(ms1, enc))
        {
          string line;
          while ((line = rdr.ReadLine()) != null)
            lines.Add(line);
        }
      }

      byte[] b2;
      using (MemoryStream ms2 = new MemoryStream())
      {
        using (StreamWriter wrt = new StreamWriter(ms2, Encoding.UTF8))
        {
          wrt.NewLine = "\r\n";
          for (int i = 0; i < lines.Count; i++)
            wrt.WriteLine(lines[i]);
          wrt.Flush();
        }
        b2 = ms2.ToArray();
      }

      if (DataTools.AreArraysEqual<byte>(b2, b1))
        return String.Empty;

      System.IO.File.WriteAllBytes(file.Path, b2);
      return "Modified";
    }

    #endregion
  }
}
