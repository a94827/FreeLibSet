namespace FreeLibSet.Forms
{
  partial class FileBrowseForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileBrowseForm));
      this.grpOptionally = new System.Windows.Forms.GroupBox();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnExplorer = new System.Windows.Forms.Button();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.lblDescription = new FreeLibSet.Controls.InfoLabel();
      this.PanelSubFolders = new System.Windows.Forms.Panel();
      this.cbSubFolders = new System.Windows.Forms.CheckBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.TextLabel = new System.Windows.Forms.Label();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.MainCB = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.grpOptionally.SuspendLayout();
      this.panel3.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.PanelSubFolders.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      resources.ApplyResources(this.ButtonsPanel, "ButtonsPanel");
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Controls.Add(this.grpOptionally);
      resources.ApplyResources(this.MainPanel, "MainPanel");
      // 
      // grpOptionally
      // 
      this.grpOptionally.Controls.Add(this.infoLabel1);
      this.grpOptionally.Controls.Add(this.panel3);
      resources.ApplyResources(this.grpOptionally, "grpOptionally");
      this.grpOptionally.Name = "grpOptionally";
      this.grpOptionally.TabStop = false;
      // 
      // infoLabel1
      // 
      this.infoLabel1.BackColor = System.Drawing.SystemColors.Control;
      resources.ApplyResources(this.infoLabel1, "infoLabel1");
      this.infoLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
      this.infoLabel1.Name = "infoLabel1";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnExplorer);
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Name = "panel3";
      // 
      // btnExplorer
      // 
      resources.ApplyResources(this.btnExplorer, "btnExplorer");
      this.btnExplorer.Name = "btnExplorer";
      this.btnExplorer.UseVisualStyleBackColor = true;
      // 
      // grpMain
      // 
      this.grpMain.Controls.Add(this.lblDescription);
      this.grpMain.Controls.Add(this.PanelSubFolders);
      this.grpMain.Controls.Add(this.panel2);
      resources.ApplyResources(this.grpMain, "grpMain");
      this.grpMain.Name = "grpMain";
      this.grpMain.TabStop = false;
      // 
      // lblDescription
      // 
      this.lblDescription.BackColor = System.Drawing.SystemColors.Control;
      resources.ApplyResources(this.lblDescription, "lblDescription");
      this.lblDescription.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblDescription.Name = "lblDescription";
      // 
      // PanelSubFolders
      // 
      this.PanelSubFolders.Controls.Add(this.cbSubFolders);
      resources.ApplyResources(this.PanelSubFolders, "PanelSubFolders");
      this.PanelSubFolders.Name = "PanelSubFolders";
      // 
      // cbSubFolders
      // 
      resources.ApplyResources(this.cbSubFolders, "cbSubFolders");
      this.cbSubFolders.Name = "cbSubFolders";
      this.cbSubFolders.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.TextLabel);
      this.panel2.Controls.Add(this.btnBrowse);
      this.panel2.Controls.Add(this.MainCB);
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Name = "panel2";
      // 
      // TextLabel
      // 
      resources.ApplyResources(this.TextLabel, "TextLabel");
      this.TextLabel.Name = "TextLabel";
      // 
      // btnBrowse
      // 
      resources.ApplyResources(this.btnBrowse, "btnBrowse");
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.UseVisualStyleBackColor = true;
      // 
      // MainCB
      // 
      resources.ApplyResources(this.MainCB, "MainCB");
      this.MainCB.FormattingEnabled = true;
      this.MainCB.Name = "MainCB";
      // 
      // FileBrowseForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Name = "FileBrowseForm";
      this.MainPanel.ResumeLayout(false);
      this.grpOptionally.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.PanelSubFolders.ResumeLayout(false);
      this.PanelSubFolders.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.GroupBox grpMain;
    public System.Windows.Forms.Button btnBrowse;
    public System.Windows.Forms.ComboBox MainCB;
    public System.Windows.Forms.Label TextLabel;
    private System.Windows.Forms.GroupBox grpOptionally;
    public System.Windows.Forms.Button btnExplorer;
    public System.Windows.Forms.Panel PanelSubFolders;
    private System.Windows.Forms.Panel panel2;
    public System.Windows.Forms.CheckBox cbSubFolders;
    private Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.Panel panel3;
    public Controls.InfoLabel lblDescription;
  }
}
