using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Config;
using AgeyevAV.IO;

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

      RegistryCfg Cfg = new RegistryCfg(@"HKEY_CURRENT_USER\Software\FreeLibSet\Bmp2Png");
      ParamForm frm = new ParamForm();
      frm.efpSrcDir.Text = Cfg.GetString("SourceDir");
      frm.efpResDir.Text = Cfg.GetString("ResDir");
      frm.efpSubDirs.Checked = Cfg.GetBool("SubDirs");
      if (EFPApp.ShowDialog(frm, true) != DialogResult.OK)
        return;
      Cfg.SetString("SourceDir", frm.efpSrcDir.Text);
      Cfg.SetString("ResDir", frm.efpResDir.Text);
      Cfg.SetBool("SubDirs", frm.efpSubDirs.Checked);

      #endregion

      int cnt=0;
      int InvalidFFCount = 0;
      using (Splash spl = new Splash(new string[] { "Поиск файлов", "Преобразование" }))
      {
        AbsPath SrcDir=new AbsPath(frm.efpSrcDir.Text);
        AbsPath ResDir = new AbsPath(frm.efpResDir.Text);
        string[] aFiles = System.IO.Directory.GetFiles(SrcDir.Path, "*.bmp",
          frm.efpSubDirs.Checked ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.AllDirectories);
        spl.Complete();
        spl.PercentMax = aFiles.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < aFiles.Length; i++)
        {
          AbsPath SrcFile = new AbsPath(aFiles[i]);
          string diff = aFiles[i].Substring(SrcDir.SlashedPath.Length);
          AbsPath ResFile = new AbsPath(ResDir, diff).ChangeExtension(".png");
          if (ConvertOneFile(SrcFile, ResFile))
            cnt++;
          else
            InvalidFFCount++;
          spl.IncPercent();
        }
      }

      EFPApp.MessageBox("Преобразовано файлов: " + cnt.ToString()+". Имеют неподходящий формат: "+InvalidFFCount.ToString());
    }

    #endregion

    #region Преобразование одного файла

    private static bool ConvertOneFile(AbsPath SrcFile, AbsPath ResFile)
    {
      Bitmap bmp = Image.FromFile(SrcFile.Path) as Bitmap;
      //if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format4bppIndexed)
      //  return false;

      bmp.MakeTransparent(Color.Magenta);
      FileTools.ForceDirs(ResFile.ParentDir);
      bmp.Save(ResFile.Path);
      return true;
    }

    #endregion
  }
}