namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupDbf
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRDataViewPageSetupDbf));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpColumns = new System.Windows.Forms.GroupBox();
      this.grColumns = new System.Windows.Forms.DataGridView();
      this.panSpbColumns = new System.Windows.Forms.Panel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.cbDbfCodePage = new System.Windows.Forms.ComboBox();
      this.lblDbfCodePage = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpColumns.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpColumns);
      this.MainPanel.Controls.Add(this.panel1);
      this.MainPanel.Name = "MainPanel";
      // 
      // grpColumns
      // 
      resources.ApplyResources(this.grpColumns, "grpColumns");
      this.grpColumns.Controls.Add(this.grColumns);
      this.grpColumns.Controls.Add(this.panSpbColumns);
      this.grpColumns.Name = "grpColumns";
      this.grpColumns.TabStop = false;
      // 
      // grColumns
      // 
      resources.ApplyResources(this.grColumns, "grColumns");
      this.grColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grColumns.Name = "grColumns";
      this.grColumns.RowTemplate.Height = 24;
      // 
      // panSpbColumns
      // 
      resources.ApplyResources(this.panSpbColumns, "panSpbColumns");
      this.panSpbColumns.Name = "panSpbColumns";
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.cbDbfCodePage);
      this.panel1.Controls.Add(this.lblDbfCodePage);
      this.panel1.Name = "panel1";
      // 
      // cbDbfCodePage
      // 
      resources.ApplyResources(this.cbDbfCodePage, "cbDbfCodePage");
      this.cbDbfCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDbfCodePage.FormattingEnabled = true;
      this.cbDbfCodePage.Name = "cbDbfCodePage";
      // 
      // lblDbfCodePage
      // 
      resources.ApplyResources(this.lblDbfCodePage, "lblDbfCodePage");
      this.lblDbfCodePage.Name = "lblDbfCodePage";
      // 
      // BRDataViewPageSetupDbf
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupDbf";
      this.MainPanel.ResumeLayout(false);
      this.grpColumns.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpColumns;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ComboBox cbDbfCodePage;
    private System.Windows.Forms.Label lblDbfCodePage;
    private System.Windows.Forms.DataGridView grColumns;
    private System.Windows.Forms.Panel panSpbColumns;
  }
}