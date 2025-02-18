namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupPrinter
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRPageSetupPrinter));
      this.panPrinter = new System.Windows.Forms.Panel();
      this.lblBackInfo = new System.Windows.Forms.Label();
      this.lblBackInfoLabel = new System.Windows.Forms.Label();
      this.btnDebug = new System.Windows.Forms.Button();
      this.lblPrinterInfo = new System.Windows.Forms.Label();
      this.btnPrinterProps = new System.Windows.Forms.Button();
      this.grpPrinter = new System.Windows.Forms.GroupBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.rbSelPrinter = new System.Windows.Forms.RadioButton();
      this.rbDefaultPrinter = new System.Windows.Forms.RadioButton();
      this.cbPrinter = new System.Windows.Forms.ComboBox();
      this.panPrinter.SuspendLayout();
      this.grpPrinter.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panPrinter
      // 
      resources.ApplyResources(this.panPrinter, "panPrinter");
      this.panPrinter.Controls.Add(this.lblBackInfo);
      this.panPrinter.Controls.Add(this.lblBackInfoLabel);
      this.panPrinter.Controls.Add(this.btnDebug);
      this.panPrinter.Controls.Add(this.lblPrinterInfo);
      this.panPrinter.Controls.Add(this.btnPrinterProps);
      this.panPrinter.Controls.Add(this.grpPrinter);
      this.panPrinter.Name = "panPrinter";
      // 
      // lblBackInfo
      // 
      resources.ApplyResources(this.lblBackInfo, "lblBackInfo");
      this.lblBackInfo.Name = "lblBackInfo";
      // 
      // lblBackInfoLabel
      // 
      resources.ApplyResources(this.lblBackInfoLabel, "lblBackInfoLabel");
      this.lblBackInfoLabel.Name = "lblBackInfoLabel";
      // 
      // btnDebug
      // 
      resources.ApplyResources(this.btnDebug, "btnDebug");
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // lblPrinterInfo
      // 
      resources.ApplyResources(this.lblPrinterInfo, "lblPrinterInfo");
      this.lblPrinterInfo.Name = "lblPrinterInfo";
      // 
      // btnPrinterProps
      // 
      resources.ApplyResources(this.btnPrinterProps, "btnPrinterProps");
      this.btnPrinterProps.Name = "btnPrinterProps";
      this.btnPrinterProps.UseVisualStyleBackColor = true;
      // 
      // grpPrinter
      // 
      resources.ApplyResources(this.grpPrinter, "grpPrinter");
      this.grpPrinter.Controls.Add(this.panel3);
      this.grpPrinter.Controls.Add(this.cbPrinter);
      this.grpPrinter.Name = "grpPrinter";
      this.grpPrinter.TabStop = false;
      // 
      // panel3
      // 
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Controls.Add(this.rbSelPrinter);
      this.panel3.Controls.Add(this.rbDefaultPrinter);
      this.panel3.Name = "panel3";
      // 
      // rbSelPrinter
      // 
      resources.ApplyResources(this.rbSelPrinter, "rbSelPrinter");
      this.rbSelPrinter.Name = "rbSelPrinter";
      this.rbSelPrinter.TabStop = true;
      this.rbSelPrinter.UseVisualStyleBackColor = true;
      // 
      // rbDefaultPrinter
      // 
      resources.ApplyResources(this.rbDefaultPrinter, "rbDefaultPrinter");
      this.rbDefaultPrinter.Name = "rbDefaultPrinter";
      this.rbDefaultPrinter.TabStop = true;
      this.rbDefaultPrinter.UseVisualStyleBackColor = true;
      // 
      // cbPrinter
      // 
      resources.ApplyResources(this.cbPrinter, "cbPrinter");
      this.cbPrinter.FormattingEnabled = true;
      this.cbPrinter.Name = "cbPrinter";
      // 
      // BRPageSetupPrinter
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panPrinter);
      this.Name = "BRPageSetupPrinter";
      this.panPrinter.ResumeLayout(false);
      this.grpPrinter.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panPrinter;
    private System.Windows.Forms.Label lblBackInfo;
    private System.Windows.Forms.Label lblBackInfoLabel;
    private System.Windows.Forms.Button btnDebug;
    private System.Windows.Forms.Label lblPrinterInfo;
    private System.Windows.Forms.Button btnPrinterProps;
    private System.Windows.Forms.GroupBox grpPrinter;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.RadioButton rbSelPrinter;
    private System.Windows.Forms.RadioButton rbDefaultPrinter;
    private System.Windows.Forms.ComboBox cbPrinter;
  }
}
