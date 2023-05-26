using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.IO;

namespace TestAbsPath
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void btnOk_Click(object Sender, EventArgs Args)
    {
      try
      {
        string s = edSrc.Text;
        AbsPath p = new AbsPath(s);
        StringBuilder sb = new StringBuilder();
        AddInfo(sb,"Исходная строка", s);
        AddInfo(sb, "Path", p.Path);
        AddInfo(sb, "SlashedPath", p.SlashedPath);
        AddInfo(sb, "QuotedPath", p.QuotedPath);
        AddInfo(sb, "Uri", p.Uri==null?"[null]":p.Uri.ToString());
        //AddInfo(sb, "???", p.Uri.AbsolutePath);
        AddInfo(sb, "FileName", p.FileName);
        AddInfo(sb, "FileNameWithoutExtension", p.FileNameWithoutExtension);
        AddInfo(sb, "Extension", p.Extension);
        AddInfo(sb, "ParentDir", p.ParentDir.Path);
        AddInfo(sb, "RootDir", p.RootDir.Path);
        AddInfo(sb, "IsNetwork", p.IsNetwork.ToString());

        edInfo.Text = sb.ToString();
      }
      catch(Exception e)
      {
        MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void AddInfo(StringBuilder sb, string Name, string Value)
    {
      sb.Append(Name.PadRight(30));
      sb.Append(": ");
      sb.Append(Value);
      sb.Append(Environment.NewLine);
    }
  }
}
