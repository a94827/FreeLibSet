namespace AgeyevAV.ExtForms.Docs
{
  partial class EFPDBxTextSearchForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnDocSel = new System.Windows.Forms.Button();
      this.ButtonsPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      this.ButtonsPanel.Controls.Add(this.btnDocSel);
      this.ButtonsPanel.Controls.SetChildIndex(this.btnDocSel, 0);
      // 
      // btnDocSel
      // 
      this.btnDocSel.Location = new System.Drawing.Point(196, 8);
      this.btnDocSel.Name = "btnDocSel";
      this.btnDocSel.Size = new System.Drawing.Size(176, 24);
      this.btnDocSel.TabIndex = 2;
      this.btnDocSel.Text = "Выборка документов";
      this.btnDocSel.UseVisualStyleBackColor = true;
      // 
      // EFPDBxTextSearchForm
      // 
      this.Name = "EFPAccDepGridSearchForm";
      this.ButtonsPanel.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    internal System.Windows.Forms.Button btnDocSel;
  }
}