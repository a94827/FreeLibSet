namespace EFPCommandItemsDemo
{
  partial class SettingsTemplateForm2
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
      this.MainPanel2 = new System.Windows.Forms.Panel();
      this.edText = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.MainPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel2
      // 
      this.MainPanel2.Controls.Add(this.edText);
      this.MainPanel2.Controls.Add(this.label1);
      this.MainPanel2.Location = new System.Drawing.Point(0, 0);
      this.MainPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.MainPanel2.Name = "MainPanel2";
      this.MainPanel2.Size = new System.Drawing.Size(334, 47);
      this.MainPanel2.TabIndex = 2;
      // 
      // edText
      // 
      this.edText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edText.Location = new System.Drawing.Point(78, 11);
      this.edText.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edText.Name = "edText";
      this.edText.Size = new System.Drawing.Size(248, 20);
      this.edText.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(18, 14);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 15);
      this.label1.TabIndex = 0;
      this.label1.Text = "Текст";
      // 
      // SettingsTemplateForm2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(367, 91);
      this.Controls.Add(this.MainPanel2);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "SettingsTemplateForm2";
      this.Text = "SettingsTemplateForm2";
      this.MainPanel2.ResumeLayout(false);
      this.MainPanel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel2;
    private System.Windows.Forms.TextBox edText;
    private System.Windows.Forms.Label label1;
  }
}
