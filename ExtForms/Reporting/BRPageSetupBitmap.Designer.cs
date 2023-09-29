namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupBitmap
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
      this.MainPanel = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbClipMargins = new System.Windows.Forms.CheckBox();
      this.cbColorFormat = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edResolution = new FreeLibSet.Controls.IntEditBox();
      this.label1 = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(406, 260);
      this.MainPanel.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbClipMargins);
      this.groupBox1.Controls.Add(this.cbColorFormat);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.edResolution);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(406, 119);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры графики";
      // 
      // cbClipMargins
      // 
      this.cbClipMargins.AutoSize = true;
      this.cbClipMargins.Location = new System.Drawing.Point(15, 88);
      this.cbClipMargins.Name = "cbClipMargins";
      this.cbClipMargins.Size = new System.Drawing.Size(102, 17);
      this.cbClipMargins.TabIndex = 6;
      this.cbClipMargins.Text = "Обре&зать поля";
      this.cbClipMargins.UseVisualStyleBackColor = true;
      // 
      // cbColorFormat
      // 
      this.cbColorFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColorFormat.FormattingEnabled = true;
      this.cbColorFormat.Location = new System.Drawing.Point(196, 57);
      this.cbColorFormat.Name = "cbColorFormat";
      this.cbColorFormat.Size = new System.Drawing.Size(193, 21);
      this.cbColorFormat.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(15, 57);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(178, 21);
      this.label2.TabIndex = 2;
      this.label2.Text = "&Формат цвета";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edResolution
      // 
      this.edResolution.Increment = 0;
      this.edResolution.Location = new System.Drawing.Point(196, 30);
      this.edResolution.Name = "edResolution";
      this.edResolution.Size = new System.Drawing.Size(117, 20);
      this.edResolution.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(15, 30);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(175, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "&Разрешение. dpi";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // BRPageSetupBitmap
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(406, 260);
      this.Controls.Add(this.MainPanel);
      this.Name = "BRPageSetupBitmap";
      this.Text = "BRPageSetupBitmap";
      this.MainPanel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox cbClipMargins;
    private System.Windows.Forms.ComboBox cbColorFormat;
    private System.Windows.Forms.Label label2;
    private Controls.IntEditBox edResolution;
    private System.Windows.Forms.Label label1;
  }
}