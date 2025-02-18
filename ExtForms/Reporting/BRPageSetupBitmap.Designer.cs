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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRPageSetupBitmap));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.cbClipMargins = new System.Windows.Forms.CheckBox();
      this.cbColorFormat = new System.Windows.Forms.ComboBox();
      this.lblColorFormat = new System.Windows.Forms.Label();
      this.edResolution = new FreeLibSet.Controls.IntEditBox();
      this.lblResolution = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Name = "MainPanel";
      // 
      // grpMain
      // 
      resources.ApplyResources(this.grpMain, "grpMain");
      this.grpMain.Controls.Add(this.cbClipMargins);
      this.grpMain.Controls.Add(this.cbColorFormat);
      this.grpMain.Controls.Add(this.lblColorFormat);
      this.grpMain.Controls.Add(this.edResolution);
      this.grpMain.Controls.Add(this.lblResolution);
      this.grpMain.Name = "grpMain";
      this.grpMain.TabStop = false;
      // 
      // cbClipMargins
      // 
      resources.ApplyResources(this.cbClipMargins, "cbClipMargins");
      this.cbClipMargins.Name = "cbClipMargins";
      this.cbClipMargins.UseVisualStyleBackColor = true;
      // 
      // cbColorFormat
      // 
      resources.ApplyResources(this.cbColorFormat, "cbColorFormat");
      this.cbColorFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColorFormat.FormattingEnabled = true;
      this.cbColorFormat.Name = "cbColorFormat";
      // 
      // lblColorFormat
      // 
      resources.ApplyResources(this.lblColorFormat, "lblColorFormat");
      this.lblColorFormat.Name = "lblColorFormat";
      // 
      // edResolution
      // 
      resources.ApplyResources(this.edResolution, "edResolution");
      this.edResolution.Name = "edResolution";
      // 
      // lblResolution
      // 
      resources.ApplyResources(this.lblResolution, "lblResolution");
      this.lblResolution.Name = "lblResolution";
      // 
      // BRPageSetupBitmap
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRPageSetupBitmap";
      this.MainPanel.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.grpMain.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpMain;
    private System.Windows.Forms.CheckBox cbClipMargins;
    private System.Windows.Forms.ComboBox cbColorFormat;
    private System.Windows.Forms.Label lblColorFormat;
    private Controls.IntEditBox edResolution;
    private System.Windows.Forms.Label lblResolution;
  }
}
