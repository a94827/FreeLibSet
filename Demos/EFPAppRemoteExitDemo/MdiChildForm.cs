using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.DependedValues;

namespace EFPAppRemoteExitDemo
{
  public partial class MdiChildForm : Form
  {
    public MdiChildForm()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      btnSave.Image = EFPApp.MainImages.Images["Save"];
      btnSave.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpSave = new EFPButton(efpForm, btnSave);
      efpSave.Click += new EventHandler(efpSave_Click);

      EFPTextBox efpText = new EFPTextBox(efpForm, edText);
      edText.TextChanged += new EventHandler(edText_TextChanged);

      ChangeInfo=new DepChangeInfoItem();
      efpForm.ChangeInfo = ChangeInfo;
    }

    void efpSave_Click(object sender, EventArgs e)
    {
      // Это всего лишь эмуляция
      ChangeInfo.Changed = false;
    }

    void edText_TextChanged(object sender, EventArgs e)
    {
      ChangeInfo.Changed = true;
    }

    /// <summary>
    /// Отслеживание изменений
    /// </summary>
    private DepChangeInfoItem ChangeInfo;

    protected override void OnFormClosing(FormClosingEventArgs Args)
    {
      base.OnFormClosing(Args);
      if (Args.Cancel)
        return;

      if (!ChangeInfo.Changed)
        return;

      switch (EFPApp.MessageBox("Данные в редакторе не были сохранены. Сохранить изменения?",
        "Подтверждение",
        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
      { 
        case DialogResult.Yes:
          ChangeInfo.Changed = false;  // как бы сохранили
          break;
        case DialogResult.No:
          break;
        default:
          Args.Cancel = true;
          break;
      }
    }
  }
}