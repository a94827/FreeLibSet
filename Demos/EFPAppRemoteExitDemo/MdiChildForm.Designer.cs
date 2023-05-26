namespace EFPAppRemoteExitDemo
{
  partial class MdiChildForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.infoLabel1 = new AgeyevAV.ExtForms.InfoLabel();
      this.edText = new System.Windows.Forms.TextBox();
      this.btnSave = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnSave);
      this.panel1.Controls.Add(this.infoLabel1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(284, 78);
      this.panel1.TabIndex = 2;
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.infoLabel1.Location = new System.Drawing.Point(0, 0);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(284, 41);
      this.infoLabel1.TabIndex = 1;
      this.infoLabel1.Text = "Эмулирует редактор текста, в котором текст мжет быть \"сохранен\"";
      // 
      // edText
      // 
      this.edText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edText.Location = new System.Drawing.Point(0, 78);
      this.edText.Multiline = true;
      this.edText.Name = "edText";
      this.edText.Size = new System.Drawing.Size(284, 184);
      this.edText.TabIndex = 3;
      // 
      // btnSave
      // 
      this.btnSave.Location = new System.Drawing.Point(12, 47);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(132, 24);
      this.btnSave.TabIndex = 2;
      this.btnSave.Text = "Сохранить";
      this.btnSave.UseVisualStyleBackColor = true;
      // 
      // MdiChildForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.edText);
      this.Controls.Add(this.panel1);
      this.Name = "MdiChildForm";
      this.Text = "Тестовое дочернее окно";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnSave;
    private AgeyevAV.ExtForms.InfoLabel infoLabel1;
    private System.Windows.Forms.TextBox edText;

  }
}
