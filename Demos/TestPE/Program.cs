using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.IO;

namespace TestPE
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

      while (true)
      {
        OpenFileDialog dlg = new OpenFileDialog();
        dlg.Title = "Выбор PE файла для извлечения информации";
        dlg.Filter = "EXE-файлы|*.exe|DLL-файлы|*.dll|Все файлы|*.*";
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        AbsPath Path=new AbsPath(dlg.FileName);
        bool? Res = FileTools.Is64bitPE(Path);
        string Text;
        if (Res.HasValue)
          Text = Res.Value ? "64 bit" : "32 bit";
        else
          Text = "Не удалось определить";

        MessageBox.Show("Файл: " + Path.Path + Environment.NewLine +
          "FileTools.Is64bitPE() - Разрядность в заголовке PE: " + Text);
      }
    }
  }
}