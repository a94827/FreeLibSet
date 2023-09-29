namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupPaper
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRPageSetupPaper));
      this.panPaper = new System.Windows.Forms.Panel();
      this.grpCenterPage = new System.Windows.Forms.GroupBox();
      this.cbCenterHorizontal = new System.Windows.Forms.CheckBox();
      this.grpDuplex = new System.Windows.Forms.GroupBox();
      this.lblDuplexInfo = new System.Windows.Forms.Label();
      this.cbDuplex = new System.Windows.Forms.CheckBox();
      this.pbSrcLandscape = new System.Windows.Forms.PictureBox();
      this.pbSrcPortrait = new System.Windows.Forms.PictureBox();
      this.grpOrientation = new System.Windows.Forms.GroupBox();
      this.pbOrientation = new System.Windows.Forms.PictureBox();
      this.rbLandscape = new System.Windows.Forms.RadioButton();
      this.rbPortrait = new System.Windows.Forms.RadioButton();
      this.grpPaperSize = new System.Windows.Forms.GroupBox();
      this.edPaperHeight = new FreeLibSet.Controls.DecimalEditBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edPaperWidth = new FreeLibSet.Controls.DecimalEditBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbPageSize = new System.Windows.Forms.ComboBox();
      this.cbCenterVertical = new System.Windows.Forms.CheckBox();
      this.panPaper.SuspendLayout();
      this.grpCenterPage.SuspendLayout();
      this.grpDuplex.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbSrcLandscape)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbSrcPortrait)).BeginInit();
      this.grpOrientation.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbOrientation)).BeginInit();
      this.grpPaperSize.SuspendLayout();
      this.SuspendLayout();
      // 
      // panPaper
      // 
      this.panPaper.Controls.Add(this.grpCenterPage);
      this.panPaper.Controls.Add(this.grpDuplex);
      this.panPaper.Controls.Add(this.pbSrcLandscape);
      this.panPaper.Controls.Add(this.pbSrcPortrait);
      this.panPaper.Controls.Add(this.grpOrientation);
      this.panPaper.Controls.Add(this.grpPaperSize);
      this.panPaper.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panPaper.Location = new System.Drawing.Point(0, 0);
      this.panPaper.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panPaper.Name = "panPaper";
      this.panPaper.Size = new System.Drawing.Size(600, 423);
      this.panPaper.TabIndex = 1;
      // 
      // grpCenterPage
      // 
      this.grpCenterPage.Controls.Add(this.cbCenterVertical);
      this.grpCenterPage.Controls.Add(this.cbCenterHorizontal);
      this.grpCenterPage.Location = new System.Drawing.Point(5, 249);
      this.grpCenterPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpCenterPage.Name = "grpCenterPage";
      this.grpCenterPage.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpCenterPage.Size = new System.Drawing.Size(459, 77);
      this.grpCenterPage.TabIndex = 11;
      this.grpCenterPage.TabStop = false;
      this.grpCenterPage.Text = "Центрировать на странице";
      // 
      // cbCenterHorizontal
      // 
      this.cbCenterHorizontal.AutoSize = true;
      this.cbCenterHorizontal.Location = new System.Drawing.Point(16, 22);
      this.cbCenterHorizontal.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbCenterHorizontal.Name = "cbCenterHorizontal";
      this.cbCenterHorizontal.Size = new System.Drawing.Size(131, 21);
      this.cbCenterHorizontal.TabIndex = 0;
      this.cbCenterHorizontal.Text = "Горизонтально";
      this.cbCenterHorizontal.UseVisualStyleBackColor = true;
      // 
      // grpDuplex
      // 
      this.grpDuplex.Controls.Add(this.lblDuplexInfo);
      this.grpDuplex.Controls.Add(this.cbDuplex);
      this.grpDuplex.Location = new System.Drawing.Point(7, 124);
      this.grpDuplex.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpDuplex.Name = "grpDuplex";
      this.grpDuplex.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpDuplex.Size = new System.Drawing.Size(457, 116);
      this.grpDuplex.TabIndex = 10;
      this.grpDuplex.TabStop = false;
      // 
      // lblDuplexInfo
      // 
      this.lblDuplexInfo.Location = new System.Drawing.Point(12, 44);
      this.lblDuplexInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblDuplexInfo.Name = "lblDuplexInfo";
      this.lblDuplexInfo.Size = new System.Drawing.Size(437, 68);
      this.lblDuplexInfo.TabIndex = 1;
      this.lblDuplexInfo.Text = "???";
      this.lblDuplexInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblDuplexInfo.UseMnemonic = false;
      // 
      // cbDuplex
      // 
      this.cbDuplex.AutoSize = true;
      this.cbDuplex.Location = new System.Drawing.Point(11, 20);
      this.cbDuplex.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbDuplex.Name = "cbDuplex";
      this.cbDuplex.Size = new System.Drawing.Size(175, 21);
      this.cbDuplex.TabIndex = 0;
      this.cbDuplex.Text = "Двусторонняя печать";
      this.cbDuplex.UseVisualStyleBackColor = true;
      // 
      // pbSrcLandscape
      // 
      this.pbSrcLandscape.BackColor = System.Drawing.Color.Transparent;
      this.pbSrcLandscape.Image = ((System.Drawing.Image)(resources.GetObject("pbSrcLandscape.Image")));
      this.pbSrcLandscape.Location = new System.Drawing.Point(523, 42);
      this.pbSrcLandscape.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.pbSrcLandscape.Name = "pbSrcLandscape";
      this.pbSrcLandscape.Size = new System.Drawing.Size(32, 32);
      this.pbSrcLandscape.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pbSrcLandscape.TabIndex = 9;
      this.pbSrcLandscape.TabStop = false;
      this.pbSrcLandscape.Visible = false;
      // 
      // pbSrcPortrait
      // 
      this.pbSrcPortrait.BackColor = System.Drawing.Color.Transparent;
      this.pbSrcPortrait.Image = ((System.Drawing.Image)(resources.GetObject("pbSrcPortrait.Image")));
      this.pbSrcPortrait.Location = new System.Drawing.Point(472, 42);
      this.pbSrcPortrait.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.pbSrcPortrait.Name = "pbSrcPortrait";
      this.pbSrcPortrait.Size = new System.Drawing.Size(32, 32);
      this.pbSrcPortrait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pbSrcPortrait.TabIndex = 8;
      this.pbSrcPortrait.TabStop = false;
      this.pbSrcPortrait.Visible = false;
      // 
      // grpOrientation
      // 
      this.grpOrientation.Controls.Add(this.pbOrientation);
      this.grpOrientation.Controls.Add(this.rbLandscape);
      this.grpOrientation.Controls.Add(this.rbPortrait);
      this.grpOrientation.Location = new System.Drawing.Point(239, 20);
      this.grpOrientation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpOrientation.Name = "grpOrientation";
      this.grpOrientation.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpOrientation.Size = new System.Drawing.Size(225, 97);
      this.grpOrientation.TabIndex = 7;
      this.grpOrientation.TabStop = false;
      this.grpOrientation.Text = "Ориентация";
      // 
      // pbOrientation
      // 
      this.pbOrientation.Location = new System.Drawing.Point(152, 23);
      this.pbOrientation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.pbOrientation.Name = "pbOrientation";
      this.pbOrientation.Size = new System.Drawing.Size(49, 40);
      this.pbOrientation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pbOrientation.TabIndex = 2;
      this.pbOrientation.TabStop = false;
      // 
      // rbLandscape
      // 
      this.rbLandscape.AutoSize = true;
      this.rbLandscape.Location = new System.Drawing.Point(16, 52);
      this.rbLandscape.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbLandscape.Name = "rbLandscape";
      this.rbLandscape.Size = new System.Drawing.Size(102, 21);
      this.rbLandscape.TabIndex = 1;
      this.rbLandscape.Text = "&Альбомная";
      this.rbLandscape.UseVisualStyleBackColor = true;
      // 
      // rbPortrait
      // 
      this.rbPortrait.AutoSize = true;
      this.rbPortrait.Location = new System.Drawing.Point(16, 23);
      this.rbPortrait.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbPortrait.Name = "rbPortrait";
      this.rbPortrait.Size = new System.Drawing.Size(87, 21);
      this.rbPortrait.TabIndex = 0;
      this.rbPortrait.Text = "К&нижная";
      this.rbPortrait.UseVisualStyleBackColor = true;
      // 
      // grpPaperSize
      // 
      this.grpPaperSize.Controls.Add(this.edPaperHeight);
      this.grpPaperSize.Controls.Add(this.label2);
      this.grpPaperSize.Controls.Add(this.edPaperWidth);
      this.grpPaperSize.Controls.Add(this.label1);
      this.grpPaperSize.Controls.Add(this.cbPageSize);
      this.grpPaperSize.Location = new System.Drawing.Point(7, 4);
      this.grpPaperSize.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpPaperSize.Name = "grpPaperSize";
      this.grpPaperSize.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpPaperSize.Size = new System.Drawing.Size(224, 113);
      this.grpPaperSize.TabIndex = 6;
      this.grpPaperSize.TabStop = false;
      this.grpPaperSize.Text = "Размер страницы";
      // 
      // edPaperHeight
      // 
      this.edPaperHeight.Format = "0.00";
      this.edPaperHeight.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edPaperHeight.Location = new System.Drawing.Point(125, 81);
      this.edPaperHeight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edPaperHeight.Name = "edPaperHeight";
      this.edPaperHeight.Size = new System.Drawing.Size(91, 22);
      this.edPaperHeight.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(16, 81);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(81, 17);
      this.label2.TabIndex = 3;
      this.label2.Text = "&Высота, см";
      // 
      // edPaperWidth
      // 
      this.edPaperWidth.Format = "0.00";
      this.edPaperWidth.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edPaperWidth.Location = new System.Drawing.Point(125, 53);
      this.edPaperWidth.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edPaperWidth.Name = "edPaperWidth";
      this.edPaperWidth.Size = new System.Drawing.Size(91, 22);
      this.edPaperWidth.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 53);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(83, 17);
      this.label1.TabIndex = 1;
      this.label1.Text = "&Ширина, см";
      // 
      // cbPageSize
      // 
      this.cbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPageSize.FormattingEnabled = true;
      this.cbPageSize.Location = new System.Drawing.Point(20, 23);
      this.cbPageSize.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbPageSize.Name = "cbPageSize";
      this.cbPageSize.Size = new System.Drawing.Size(195, 24);
      this.cbPageSize.TabIndex = 0;
      // 
      // cbCenterVertical
      // 
      this.cbCenterVertical.AutoSize = true;
      this.cbCenterVertical.Location = new System.Drawing.Point(16, 48);
      this.cbCenterVertical.Margin = new System.Windows.Forms.Padding(4);
      this.cbCenterVertical.Name = "cbCenterVertical";
      this.cbCenterVertical.Size = new System.Drawing.Size(116, 21);
      this.cbCenterVertical.TabIndex = 1;
      this.cbCenterVertical.Text = "Вертикально";
      this.cbCenterVertical.UseVisualStyleBackColor = true;
      // 
      // BRPageSetupPaper
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(600, 423);
      this.Controls.Add(this.panPaper);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "BRPageSetupPaper";
      this.Text = "BRPageSetupPaper";
      this.panPaper.ResumeLayout(false);
      this.panPaper.PerformLayout();
      this.grpCenterPage.ResumeLayout(false);
      this.grpCenterPage.PerformLayout();
      this.grpDuplex.ResumeLayout(false);
      this.grpDuplex.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbSrcLandscape)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.pbSrcPortrait)).EndInit();
      this.grpOrientation.ResumeLayout(false);
      this.grpOrientation.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbOrientation)).EndInit();
      this.grpPaperSize.ResumeLayout(false);
      this.grpPaperSize.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panPaper;
    private System.Windows.Forms.GroupBox grpCenterPage;
    private System.Windows.Forms.CheckBox cbCenterHorizontal;
    private System.Windows.Forms.GroupBox grpDuplex;
    private System.Windows.Forms.Label lblDuplexInfo;
    private System.Windows.Forms.CheckBox cbDuplex;
    private System.Windows.Forms.PictureBox pbSrcLandscape;
    private System.Windows.Forms.PictureBox pbSrcPortrait;
    private System.Windows.Forms.GroupBox grpOrientation;
    private System.Windows.Forms.PictureBox pbOrientation;
    private System.Windows.Forms.RadioButton rbLandscape;
    private System.Windows.Forms.RadioButton rbPortrait;
    private System.Windows.Forms.GroupBox grpPaperSize;
    private FreeLibSet.Controls.DecimalEditBox edPaperHeight;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.DecimalEditBox edPaperWidth;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbPageSize;
    private System.Windows.Forms.CheckBox cbCenterVertical;
  }
}