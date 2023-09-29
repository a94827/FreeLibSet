namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupFont
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
      this.panFont = new System.Windows.Forms.Panel();
      this.grpFontWidth = new System.Windows.Forms.GroupBox();
      this.edFontWidth = new FreeLibSet.Controls.SingleEditBox();
      this.grpFontHeight = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.edFontHeight = new FreeLibSet.Controls.SingleEditBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.pbFontSample = new System.Windows.Forms.PictureBox();
      this.label7 = new System.Windows.Forms.Label();
      this.cbFontName = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbWidthAuto = new System.Windows.Forms.RadioButton();
      this.rbWidthFixed = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.rbLineFixed = new System.Windows.Forms.RadioButton();
      this.rbLineAuto = new System.Windows.Forms.RadioButton();
      this.edLineHeight = new FreeLibSet.Controls.SingleEditBox();
      this.panFont.SuspendLayout();
      this.grpFontWidth.SuspendLayout();
      this.grpFontHeight.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbFontSample)).BeginInit();
      this.panel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panFont
      // 
      this.panFont.Controls.Add(this.groupBox2);
      this.panFont.Controls.Add(this.grpFontWidth);
      this.panFont.Controls.Add(this.grpFontHeight);
      this.panFont.Controls.Add(this.groupBox1);
      this.panFont.Controls.Add(this.label7);
      this.panFont.Controls.Add(this.cbFontName);
      this.panFont.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panFont.Location = new System.Drawing.Point(0, 0);
      this.panFont.Name = "panFont";
      this.panFont.Size = new System.Drawing.Size(531, 392);
      this.panFont.TabIndex = 1;
      // 
      // grpFontWidth
      // 
      this.grpFontWidth.Controls.Add(this.panel1);
      this.grpFontWidth.Controls.Add(this.edFontWidth);
      this.grpFontWidth.Location = new System.Drawing.Point(233, 38);
      this.grpFontWidth.Name = "grpFontWidth";
      this.grpFontWidth.Size = new System.Drawing.Size(220, 72);
      this.grpFontWidth.TabIndex = 3;
      this.grpFontWidth.TabStop = false;
      this.grpFontWidth.Text = "Ш&ирина шрифта";
      // 
      // edFontWidth
      // 
      this.edFontWidth.Format = "0.0";
      this.edFontWidth.Increment = 0F;
      this.edFontWidth.Location = new System.Drawing.Point(144, 42);
      this.edFontWidth.Name = "edFontWidth";
      this.edFontWidth.Size = new System.Drawing.Size(70, 20);
      this.edFontWidth.TabIndex = 1;
      // 
      // grpFontHeight
      // 
      this.grpFontHeight.Controls.Add(this.label8);
      this.grpFontHeight.Controls.Add(this.edFontHeight);
      this.grpFontHeight.Location = new System.Drawing.Point(7, 38);
      this.grpFontHeight.Name = "grpFontHeight";
      this.grpFontHeight.Size = new System.Drawing.Size(220, 72);
      this.grpFontHeight.TabIndex = 2;
      this.grpFontHeight.TabStop = false;
      this.grpFontHeight.Text = "В&ысота шрифта";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(6, 41);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(76, 20);
      this.label8.TabIndex = 0;
      this.label8.Text = "Значение, пт";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edFontHeight
      // 
      this.edFontHeight.Format = "0.0";
      this.edFontHeight.Increment = 0F;
      this.edFontHeight.Location = new System.Drawing.Point(144, 42);
      this.edFontHeight.Name = "edFontHeight";
      this.edFontHeight.Size = new System.Drawing.Size(70, 20);
      this.edFontHeight.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.pbFontSample);
      this.groupBox1.Location = new System.Drawing.Point(8, 194);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(518, 191);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Образец";
      // 
      // pbFontSample
      // 
      this.pbFontSample.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbFontSample.BackColor = System.Drawing.Color.White;
      this.pbFontSample.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pbFontSample.Location = new System.Drawing.Point(6, 19);
      this.pbFontSample.Name = "pbFontSample";
      this.pbFontSample.Size = new System.Drawing.Size(506, 166);
      this.pbFontSample.TabIndex = 7;
      this.pbFontSample.TabStop = false;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(5, 10);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(84, 21);
      this.label7.TabIndex = 0;
      this.label7.Text = "&Шрифт";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbFontName
      // 
      this.cbFontName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFontName.FormattingEnabled = true;
      this.cbFontName.Location = new System.Drawing.Point(95, 10);
      this.cbFontName.Name = "cbFontName";
      this.cbFontName.Size = new System.Drawing.Size(425, 21);
      this.cbFontName.TabIndex = 1;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.rbWidthFixed);
      this.panel1.Controls.Add(this.rbWidthAuto);
      this.panel1.Location = new System.Drawing.Point(16, 19);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(104, 46);
      this.panel1.TabIndex = 0;
      // 
      // rbWidthAuto
      // 
      this.rbWidthAuto.AutoSize = true;
      this.rbWidthAuto.Location = new System.Drawing.Point(8, 3);
      this.rbWidthAuto.Name = "rbWidthAuto";
      this.rbWidthAuto.Size = new System.Drawing.Size(49, 17);
      this.rbWidthAuto.TabIndex = 0;
      this.rbWidthAuto.TabStop = true;
      this.rbWidthAuto.Text = "Авто";
      this.rbWidthAuto.UseVisualStyleBackColor = true;
      // 
      // rbWidthFixed
      // 
      this.rbWidthFixed.AutoSize = true;
      this.rbWidthFixed.Location = new System.Drawing.Point(8, 26);
      this.rbWidthFixed.Name = "rbWidthFixed";
      this.rbWidthFixed.Size = new System.Drawing.Size(90, 17);
      this.rbWidthFixed.TabIndex = 1;
      this.rbWidthFixed.TabStop = true;
      this.rbWidthFixed.Text = "Значение, пт";
      this.rbWidthFixed.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.panel2);
      this.groupBox2.Controls.Add(this.edLineHeight);
      this.groupBox2.Location = new System.Drawing.Point(8, 116);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(220, 72);
      this.groupBox2.TabIndex = 4;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Высота строки";
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.rbLineFixed);
      this.panel2.Controls.Add(this.rbLineAuto);
      this.panel2.Location = new System.Drawing.Point(16, 19);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(104, 46);
      this.panel2.TabIndex = 0;
      // 
      // rbLineFixed
      // 
      this.rbLineFixed.AutoSize = true;
      this.rbLineFixed.Location = new System.Drawing.Point(8, 26);
      this.rbLineFixed.Name = "rbLineFixed";
      this.rbLineFixed.Size = new System.Drawing.Size(90, 17);
      this.rbLineFixed.TabIndex = 1;
      this.rbLineFixed.TabStop = true;
      this.rbLineFixed.Text = "Значение, пт";
      this.rbLineFixed.UseVisualStyleBackColor = true;
      // 
      // rbLineAuto
      // 
      this.rbLineAuto.AutoSize = true;
      this.rbLineAuto.Location = new System.Drawing.Point(8, 3);
      this.rbLineAuto.Name = "rbLineAuto";
      this.rbLineAuto.Size = new System.Drawing.Size(49, 17);
      this.rbLineAuto.TabIndex = 0;
      this.rbLineAuto.TabStop = true;
      this.rbLineAuto.Text = "Авто";
      this.rbLineAuto.UseVisualStyleBackColor = true;
      // 
      // edLineHeight
      // 
      this.edLineHeight.Format = "0.0";
      this.edLineHeight.Increment = 0F;
      this.edLineHeight.Location = new System.Drawing.Point(144, 42);
      this.edLineHeight.Name = "edLineHeight";
      this.edLineHeight.Size = new System.Drawing.Size(70, 20);
      this.edLineHeight.TabIndex = 1;
      // 
      // BRPageSetupFont
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(531, 392);
      this.Controls.Add(this.panFont);
      this.Name = "BRPageSetupFont";
      this.Text = "BRPageSetupFont";
      this.panFont.ResumeLayout(false);
      this.grpFontWidth.ResumeLayout(false);
      this.grpFontHeight.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbFontSample)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panFont;
    private System.Windows.Forms.GroupBox grpFontWidth;
    private FreeLibSet.Controls.SingleEditBox edFontWidth;
    private System.Windows.Forms.GroupBox grpFontHeight;
    private System.Windows.Forms.Label label8;
    private FreeLibSet.Controls.SingleEditBox edFontHeight;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.PictureBox pbFontSample;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox cbFontName;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.RadioButton rbLineFixed;
    private System.Windows.Forms.RadioButton rbLineAuto;
    private Controls.SingleEditBox edLineHeight;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbWidthFixed;
    private System.Windows.Forms.RadioButton rbWidthAuto;
  }
}