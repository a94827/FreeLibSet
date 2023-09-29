using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Shell;

namespace TestFileAssociations
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      try
      {
        EFPApp.InitApp();
        /*
        TextInputDialog dlg1 = new TextInputDialog();
        dlg1.CanBeEmpty = false;
        dlg1.Prompt = "Расширение";
        if (dlg1.ShowDialog() != DialogResult.OK)
          return;

        string fileExt = dlg1.Text;
        */
        string fileExt = ".txt";
        if (fileExt[0] != '.')
          fileExt = "." + fileExt;

        FileAssociations FAs;

        DateTime startTime = DateTime.Now;
        using (Splash spl=new Splash("Получение файловых ассоциаций для "+fileExt))
        {
          FAs = FileAssociations.FromFileExtension(fileExt);
        }
        TimeSpan time = DateTime.Now - startTime;
        ShowFAs(FAs, "Расширение \"" + fileExt + "\" ("+time.ToString()+")");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    private static void ShowFAs(FileAssociations FAs, string title)
    {
      SimpleGridForm form = new SimpleGridForm();
      form.Text = title;
      EFPDataGridView efpGr = new EFPDataGridView(form);
      efpGr.Control.AutoGenerateColumns = false;
      efpGr.Control.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
      efpGr.Columns.AddImage("SmallImage");
      efpGr.Columns.LastAdded.GridColumn.ToolTipText = "Маленький значок";
      efpGr.Columns.AddText("DisplayName", true, "DisplayName", 20, 10);
      efpGr.Columns.AddImage("BigImage");
      efpGr.Columns.LastAdded.GridColumn.ToolTipText = "Большой значок";
      efpGr.Columns.LastAdded.TextWidth = 5;
      efpGr.Columns.AddText("ProgId", true, "ProgId", 15, 5);
      efpGr.Columns.AddText("ProgramPath", true, "ProgramPath", 40, 10);
      efpGr.Columns.AddText("Arguments", true, "Arguments", 10, 5);
      efpGr.Columns.AddText("IconPath", true, "IconPath", 40, 10);
      efpGr.Columns.AddInt("IconIndex", true, "IconIndex", 4);
      efpGr.Columns.AddBool("UseURL", true, "UseURL");
      efpGr.Columns.AddText("InfoSourceString", true, "InfoSourceString", 50, 10); // Только в отладочном режиме
      efpGr.DisableOrdering();
      efpGr.FrozenColumns = 2;
      efpGr.GetCellAttributes += EfpGr_GetCellAttributes;
      efpGr.Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      efpGr.Control.ReadOnly = true;
      efpGr.ReadOnly = true;
      efpGr.CanView = false;
      efpGr.Control.DataSource = FAs;
      Application.Run(form);
    }

    private static void EfpGr_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      FileAssociationItem fa = args.DataBoundItem as FileAssociationItem;
      if (fa == null)
        return;
      if (args.ColumnIndex == 0 || args.ColumnIndex == 2)
      {
        try
        {
          if (fa.IconPath.IsEmpty)
            args.Value = EFPApp.MainImages.Images["EmptyImage"];
          else
          {
            args.Value = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, args.ColumnIndex==0);
          }
        }
        catch
        {
        }
      }
    }
  }
}
