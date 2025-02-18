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
      this.cbCenterVertical = new System.Windows.Forms.CheckBox();
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
      this.lblPaperHeight = new System.Windows.Forms.Label();
      this.edPaperWidth = new FreeLibSet.Controls.DecimalEditBox();
      this.lblPaperWidth = new System.Windows.Forms.Label();
      this.cbPageSize = new System.Windows.Forms.ComboBox();
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
      resources.ApplyResources(this.panPaper, "panPaper");
      this.panPaper.Controls.Add(this.grpCenterPage);
      this.panPaper.Controls.Add(this.grpDuplex);
      this.panPaper.Controls.Add(this.pbSrcLandscape);
      this.panPaper.Controls.Add(this.pbSrcPortrait);
      this.panPaper.Controls.Add(this.grpOrientation);
      this.panPaper.Controls.Add(this.grpPaperSize);
      this.panPaper.Name = "panPaper";
      // 
      // grpCenterPage
      // 
      resources.ApplyResources(this.grpCenterPage, "grpCenterPage");
      this.grpCenterPage.Controls.Add(this.cbCenterVertical);
      this.grpCenterPage.Controls.Add(this.cbCenterHorizontal);
      this.grpCenterPage.Name = "grpCenterPage";
      this.grpCenterPage.TabStop = false;
      // 
      // cbCenterVertical
      // 
      resources.ApplyResources(this.cbCenterVertical, "cbCenterVertical");
      this.cbCenterVertical.Name = "cbCenterVertical";
      this.cbCenterVertical.UseVisualStyleBackColor = true;
      // 
      // cbCenterHorizontal
      // 
      resources.ApplyResources(this.cbCenterHorizontal, "cbCenterHorizontal");
      this.cbCenterHorizontal.Name = "cbCenterHorizontal";
      this.cbCenterHorizontal.UseVisualStyleBackColor = true;
      // 
      // grpDuplex
      // 
      resources.ApplyResources(this.grpDuplex, "grpDuplex");
      this.grpDuplex.Controls.Add(this.lblDuplexInfo);
      this.grpDuplex.Controls.Add(this.cbDuplex);
      this.grpDuplex.Name = "grpDuplex";
      this.grpDuplex.TabStop = false;
      // 
      // lblDuplexInfo
      // 
      resources.ApplyResources(this.lblDuplexInfo, "lblDuplexInfo");
      this.lblDuplexInfo.Name = "lblDuplexInfo";
      this.lblDuplexInfo.UseMnemonic = false;
      // 
      // cbDuplex
      // 
      resources.ApplyResources(this.cbDuplex, "cbDuplex");
      this.cbDuplex.Name = "cbDuplex";
      this.cbDuplex.UseVisualStyleBackColor = true;
      // 
      // pbSrcLandscape
      // 
      resources.ApplyResources(this.pbSrcLandscape, "pbSrcLandscape");
      this.pbSrcLandscape.BackColor = System.Drawing.Color.Transparent;
      this.pbSrcLandscape.Name = "pbSrcLandscape";
      this.pbSrcLandscape.TabStop = false;
      // 
      // pbSrcPortrait
      // 
      resources.ApplyResources(this.pbSrcPortrait, "pbSrcPortrait");
      this.pbSrcPortrait.BackColor = System.Drawing.Color.Transparent;
      this.pbSrcPortrait.Name = "pbSrcPortrait";
      this.pbSrcPortrait.TabStop = false;
      // 
      // grpOrientation
      // 
      resources.ApplyResources(this.grpOrientation, "grpOrientation");
      this.grpOrientation.Controls.Add(this.pbOrientation);
      this.grpOrientation.Controls.Add(this.rbLandscape);
      this.grpOrientation.Controls.Add(this.rbPortrait);
      this.grpOrientation.Name = "grpOrientation";
      this.grpOrientation.TabStop = false;
      // 
      // pbOrientation
      // 
      resources.ApplyResources(this.pbOrientation, "pbOrientation");
      this.pbOrientation.Name = "pbOrientation";
      this.pbOrientation.TabStop = false;
      // 
      // rbLandscape
      // 
      resources.ApplyResources(this.rbLandscape, "rbLandscape");
      this.rbLandscape.Name = "rbLandscape";
      this.rbLandscape.UseVisualStyleBackColor = true;
      // 
      // rbPortrait
      // 
      resources.ApplyResources(this.rbPortrait, "rbPortrait");
      this.rbPortrait.Name = "rbPortrait";
      this.rbPortrait.UseVisualStyleBackColor = true;
      // 
      // grpPaperSize
      // 
      resources.ApplyResources(this.grpPaperSize, "grpPaperSize");
      this.grpPaperSize.Controls.Add(this.edPaperHeight);
      this.grpPaperSize.Controls.Add(this.lblPaperHeight);
      this.grpPaperSize.Controls.Add(this.edPaperWidth);
      this.grpPaperSize.Controls.Add(this.lblPaperWidth);
      this.grpPaperSize.Controls.Add(this.cbPageSize);
      this.grpPaperSize.Name = "grpPaperSize";
      this.grpPaperSize.TabStop = false;
      // 
      // edPaperHeight
      // 
      resources.ApplyResources(this.edPaperHeight, "edPaperHeight");
      this.edPaperHeight.Format = "0.00";
      this.edPaperHeight.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edPaperHeight.Name = "edPaperHeight";
      // 
      // lblPaperHeight
      // 
      resources.ApplyResources(this.lblPaperHeight, "lblPaperHeight");
      this.lblPaperHeight.Name = "lblPaperHeight";
      // 
      // edPaperWidth
      // 
      resources.ApplyResources(this.edPaperWidth, "edPaperWidth");
      this.edPaperWidth.Format = "0.00";
      this.edPaperWidth.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edPaperWidth.Name = "edPaperWidth";
      // 
      // lblPaperWidth
      // 
      resources.ApplyResources(this.lblPaperWidth, "lblPaperWidth");
      this.lblPaperWidth.Name = "lblPaperWidth";
      // 
      // cbPageSize
      // 
      resources.ApplyResources(this.cbPageSize, "cbPageSize");
      this.cbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPageSize.FormattingEnabled = true;
      this.cbPageSize.Name = "cbPageSize";
      // 
      // BRPageSetupPaper
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panPaper);
      this.Name = "BRPageSetupPaper";
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
    private System.Windows.Forms.Label lblPaperHeight;
    private FreeLibSet.Controls.DecimalEditBox edPaperWidth;
    private System.Windows.Forms.Label lblPaperWidth;
    private System.Windows.Forms.ComboBox cbPageSize;
    private System.Windows.Forms.CheckBox cbCenterVertical;
  }
}
