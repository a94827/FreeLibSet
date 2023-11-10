namespace EFPCommandItemsDemo
{
  partial class SettingsTemplateForm1
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
      this.MainPanel1 = new System.Windows.Forms.Panel();
      this.cbOption2 = new System.Windows.Forms.CheckBox();
      this.cbOption1 = new System.Windows.Forms.CheckBox();
      this.MainPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel1
      // 
      this.MainPanel1.Controls.Add(this.cbOption2);
      this.MainPanel1.Controls.Add(this.cbOption1);
      this.MainPanel1.Location = new System.Drawing.Point(12, 12);
      this.MainPanel1.Name = "MainPanel1";
      this.MainPanel1.Size = new System.Drawing.Size(159, 78);
      this.MainPanel1.TabIndex = 0;
      // 
      // cbOption2
      // 
      this.cbOption2.AutoSize = true;
      this.cbOption2.Location = new System.Drawing.Point(15, 45);
      this.cbOption2.Name = "cbOption2";
      this.cbOption2.Size = new System.Drawing.Size(85, 21);
      this.cbOption2.TabIndex = 1;
      this.cbOption2.Text = "Опция 2";
      this.cbOption2.UseVisualStyleBackColor = true;
      // 
      // cbOption1
      // 
      this.cbOption1.AutoSize = true;
      this.cbOption1.Location = new System.Drawing.Point(15, 18);
      this.cbOption1.Name = "cbOption1";
      this.cbOption1.Size = new System.Drawing.Size(85, 21);
      this.cbOption1.TabIndex = 0;
      this.cbOption1.Text = "Опция 1";
      this.cbOption1.UseVisualStyleBackColor = true;
      // 
      // SettingsTemplateForm1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(494, 352);
      this.Controls.Add(this.MainPanel1);
      this.Name = "SettingsTemplateForm1";
      this.Text = "SettingsTemplateForm";
      this.MainPanel1.ResumeLayout(false);
      this.MainPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel1;
    private System.Windows.Forms.CheckBox cbOption2;
    private System.Windows.Forms.CheckBox cbOption1;
  }
}
