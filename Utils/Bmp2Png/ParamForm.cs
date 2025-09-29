using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Config;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace Bmp2Png
{
  public partial class ParamForm : Form
  {
    #region Конструктор формы

    public ParamForm()
    {
      InitializeComponent();
      Icon = WinFormsTools.AppIcon;
      EFPFormProvider efpForm = new EFPFormProvider(this);

      efpSrcDir = new EFPTextBox(efpForm, edSrcDir);
      efpSrcDir.CanBeEmpty = false;
      EFPFolderBrowserButton efpBrowseSrcDir = new EFPFolderBrowserButton(efpSrcDir, btnBrowseSrcDir);
      efpBrowseSrcDir.ShowNewFolderButton = false;

      efpResDir = new EFPTextBox(efpForm, edResDir);
      efpResDir.CanBeEmpty = false;
      EFPFolderBrowserButton efpBrowseResDir = new EFPFolderBrowserButton(efpResDir, btnBrowseResDir);
      efpBrowseResDir.ShowNewFolderButton = true;

      efpSubDirs = new EFPCheckBox(efpForm, cbSubDirs);
    }

    #endregion

    #region Поля

    public EFPTextBox efpSrcDir, efpResDir;
    public EFPCheckBox efpSubDirs;

    #endregion

    #region Статический метод запуска

    public static void PerformConvert()
    {
      #region Запрос параметров

      RegistryCfg cfg = new RegistryCfg(@"HKEY_CURRENT_USER\Software\FreeLibSet\Bmp2Png");
      ParamForm frm = new ParamForm();
      frm.efpSrcDir.Text = cfg.GetString("SourceDir");
      frm.efpResDir.Text = cfg.GetString("ResDir");
      frm.efpSubDirs.Checked = cfg.GetBoolean("SubDirs");
      if (EFPApp.ShowDialog(frm, true) != DialogResult.OK)
        return;
      cfg.SetString("SourceDir", frm.efpSrcDir.Text);
      cfg.SetString("ResDir", frm.efpResDir.Text);
      cfg.SetBoolean("SubDirs", frm.efpSubDirs.Checked);

      #endregion

      int cnt = 0;
      int invalidFFCount = 0;
      using (Splash spl = new Splash(new string[] { "Поиск файлов", "Преобразование" }))
      {
        AbsPath srcDir = new AbsPath(frm.efpSrcDir.Text);
        AbsPath resDir = new AbsPath(frm.efpResDir.Text);
        string[] aFiles1 = System.IO.Directory.GetFiles(srcDir.Path, "*.bmp",
          frm.efpSubDirs.Checked ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.AllDirectories);
        string[] aFiles2 = System.IO.Directory.GetFiles(srcDir.Path, "*.ico",
          frm.efpSubDirs.Checked ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.AllDirectories);
        string[] aFiles = ArrayTools.MergeArrays<string>(aFiles1, aFiles2);
        spl.Complete();
        spl.PercentMax = aFiles.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < aFiles.Length; i++)
        {
          AbsPath srcFile = new AbsPath(aFiles[i]);
          string diff = aFiles[i].Substring(srcDir.SlashedPath.Length);
          AbsPath resFile = new AbsPath(resDir, diff).ChangeExtension(".png");
          if (ConvertOneFile(srcFile, resFile))
            cnt++;
          else
            invalidFFCount++;
          spl.IncPercent();
        }
      }

      EFPApp.MessageBox("Преобразовано файлов: " + cnt.ToString() + ". Имеют неподходящий формат: " + invalidFFCount.ToString());
    }

    #endregion

    #region Преобразование одного файла

    private static bool ConvertOneFile(AbsPath srcFile, AbsPath resFile)
    {
      Bitmap bmp = Image.FromFile(srcFile.Path) as Bitmap;
      //if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format4bppIndexed)
      //  return false;

      if (String.Equals(srcFile.Extension, ".bmp", StringComparison.OrdinalIgnoreCase))
        bmp.MakeTransparent(Color.Magenta);
      FileTools.ForceDirs(resFile.ParentDir);
      bmp.Save(resFile.Path);
      return true;
    }

    #endregion
  }
}
