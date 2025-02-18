namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupSendTo
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRDataViewPageSetupSendTo));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.lblInfo = new FreeLibSet.Controls.InfoLabel();
      this.grpArea = new System.Windows.Forms.GroupBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.rbSelected = new System.Windows.Forms.RadioButton();
      this.rbAll = new System.Windows.Forms.RadioButton();
      this.cbExpTableFilters = new System.Windows.Forms.CheckBox();
      this.cbExpTableHeader = new System.Windows.Forms.CheckBox();
      this.cbExpColumnHeaders = new System.Windows.Forms.CheckBox();
      this.MainPanel.SuspendLayout();
      this.grpArea.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.lblInfo);
      this.MainPanel.Controls.Add(this.grpArea);
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Name = "MainPanel";
      // 
      // lblInfo
      // 
      resources.ApplyResources(this.lblInfo, "lblInfo");
      this.lblInfo.Name = "lblInfo";
      // 
      // grpArea
      // 
      this.grpArea.Controls.Add(this.panel2);
      this.grpArea.Controls.Add(this.cbExpTableFilters);
      this.grpArea.Controls.Add(this.cbExpTableHeader);
      this.grpArea.Controls.Add(this.cbExpColumnHeaders);
      resources.ApplyResources(this.grpArea, "grpArea");
      this.grpArea.Name = "grpArea";
      this.grpArea.TabStop = false;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.rbSelected);
      this.panel2.Controls.Add(this.rbAll);
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Name = "panel2";
      // 
      // rbSelected
      // 
      resources.ApplyResources(this.rbSelected, "rbSelected");
      this.rbSelected.Name = "rbSelected";
      this.rbSelected.TabStop = true;
      this.rbSelected.UseVisualStyleBackColor = true;
      // 
      // rbAll
      // 
      resources.ApplyResources(this.rbAll, "rbAll");
      this.rbAll.Name = "rbAll";
      this.rbAll.TabStop = true;
      this.rbAll.UseVisualStyleBackColor = true;
      // 
      // cbExpTableFilters
      // 
      resources.ApplyResources(this.cbExpTableFilters, "cbExpTableFilters");
      this.cbExpTableFilters.Name = "cbExpTableFilters";
      this.cbExpTableFilters.UseVisualStyleBackColor = true;
      // 
      // cbExpTableHeader
      // 
      resources.ApplyResources(this.cbExpTableHeader, "cbExpTableHeader");
      this.cbExpTableHeader.Name = "cbExpTableHeader";
      this.cbExpTableHeader.UseVisualStyleBackColor = true;
      // 
      // cbExpColumnHeaders
      // 
      resources.ApplyResources(this.cbExpColumnHeaders, "cbExpColumnHeaders");
      this.cbExpColumnHeaders.Name = "cbExpColumnHeaders";
      this.cbExpColumnHeaders.UseVisualStyleBackColor = true;
      // 
      // BRDataViewPageSetupSendTo
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupSendTo";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.grpArea.ResumeLayout(false);
      this.grpArea.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpArea;
    private Controls.InfoLabel lblInfo;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.RadioButton rbSelected;
    private System.Windows.Forms.RadioButton rbAll;
    private System.Windows.Forms.CheckBox cbExpTableFilters;
    private System.Windows.Forms.CheckBox cbExpTableHeader;
    private System.Windows.Forms.CheckBox cbExpColumnHeaders;
  }
}
