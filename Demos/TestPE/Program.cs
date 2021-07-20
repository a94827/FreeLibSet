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
        dlg.Title = "����� PE ����� ��� ���������� ����������";
        dlg.Filter = "EXE-�����|*.exe|DLL-�����|*.dll|��� �����|*.*";
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        AbsPath Path=new AbsPath(dlg.FileName);
        bool? Res = FileTools.Is64bitPE(Path);
        string Text;
        if (Res.HasValue)
          Text = Res.Value ? "64 bit" : "32 bit";
        else
          Text = "�� ������� ����������";

        MessageBox.Show("����: " + Path.Path + Environment.NewLine +
          "FileTools.Is64bitPE() - ����������� � ��������� PE: " + Text);
      }
    }
  }
}