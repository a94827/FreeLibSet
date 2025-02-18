namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupColumns
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRDataViewPageSetupColumns));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.grColumns = new System.Windows.Forms.DataGridView();
      this.panSpbColumns = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.lblColumnSubHeaderNumbers = new System.Windows.Forms.Label();
      this.lblWholeWidth = new System.Windows.Forms.Label();
      this.lblRepeatColumns = new System.Windows.Forms.Label();
      this.lblWorkWidth = new System.Windows.Forms.Label();
      this.lblWholeWidthValueText = new System.Windows.Forms.Label();
      this.lblWorkWidthValueText = new System.Windows.Forms.Label();
      this.edRepeatColumns = new FreeLibSet.Controls.IntEditBox();
      this.cbColumnSubHeaderNumbers = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.panel1);
      this.MainPanel.Controls.Add(this.tableLayoutPanel1);
      this.MainPanel.Name = "MainPanel";
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.grColumns);
      this.panel1.Controls.Add(this.panSpbColumns);
      this.panel1.Name = "panel1";
      // 
      // grColumns
      // 
      resources.ApplyResources(this.grColumns, "grColumns");
      this.grColumns.AllowUserToAddRows = false;
      this.grColumns.AllowUserToDeleteRows = false;
      this.grColumns.AllowUserToResizeColumns = false;
      this.grColumns.AllowUserToResizeRows = false;
      this.grColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grColumns.Name = "grColumns";
      this.grColumns.RowHeadersVisible = false;
      this.grColumns.StandardTab = true;
      // 
      // panSpbColumns
      // 
      resources.ApplyResources(this.panSpbColumns, "panSpbColumns");
      this.panSpbColumns.Name = "panSpbColumns";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblColumnSubHeaderNumbers, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.lblWholeWidth, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblRepeatColumns, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.lblWorkWidth, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.lblWholeWidthValueText, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblWorkWidthValueText, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.edRepeatColumns, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.cbColumnSubHeaderNumbers, 1, 4);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // lblColumnSubHeaderNumbers
      // 
      resources.ApplyResources(this.lblColumnSubHeaderNumbers, "lblColumnSubHeaderNumbers");
      this.lblColumnSubHeaderNumbers.Name = "lblColumnSubHeaderNumbers";
      // 
      // lblWholeWidth
      // 
      resources.ApplyResources(this.lblWholeWidth, "lblWholeWidth");
      this.tableLayoutPanel1.SetColumnSpan(this.lblWholeWidth, 2);
      this.lblWholeWidth.Name = "lblWholeWidth";
      // 
      // lblRepeatColumns
      // 
      resources.ApplyResources(this.lblRepeatColumns, "lblRepeatColumns");
      this.lblRepeatColumns.Name = "lblRepeatColumns";
      // 
      // lblWorkWidth
      // 
      resources.ApplyResources(this.lblWorkWidth, "lblWorkWidth");
      this.tableLayoutPanel1.SetColumnSpan(this.lblWorkWidth, 2);
      this.lblWorkWidth.Name = "lblWorkWidth";
      // 
      // lblWholeWidthValueText
      // 
      resources.ApplyResources(this.lblWholeWidthValueText, "lblWholeWidthValueText");
      this.lblWholeWidthValueText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblWholeWidthValueText.Name = "lblWholeWidthValueText";
      // 
      // lblWorkWidthValueText
      // 
      resources.ApplyResources(this.lblWorkWidthValueText, "lblWorkWidthValueText");
      this.lblWorkWidthValueText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblWorkWidthValueText.Name = "lblWorkWidthValueText";
      // 
      // edRepeatColumns
      // 
      resources.ApplyResources(this.edRepeatColumns, "edRepeatColumns");
      this.edRepeatColumns.Increment = 1;
      this.edRepeatColumns.Name = "edRepeatColumns";
      // 
      // cbColumnSubHeaderNumbers
      // 
      resources.ApplyResources(this.cbColumnSubHeaderNumbers, "cbColumnSubHeaderNumbers");
      this.tableLayoutPanel1.SetColumnSpan(this.cbColumnSubHeaderNumbers, 2);
      this.cbColumnSubHeaderNumbers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColumnSubHeaderNumbers.FormattingEnabled = true;
      this.cbColumnSubHeaderNumbers.Items.AddRange(new object[] {
            resources.GetString("cbColumnSubHeaderNumbers.Items"),
            resources.GetString("cbColumnSubHeaderNumbers.Items1"),
            resources.GetString("cbColumnSubHeaderNumbers.Items2")});
      this.cbColumnSubHeaderNumbers.Name = "cbColumnSubHeaderNumbers";
      // 
      // BRDataViewPageSetupColumns
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupColumns";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Panel panel1;
    public System.Windows.Forms.DataGridView grColumns;
    public System.Windows.Forms.Panel panSpbColumns;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    public System.Windows.Forms.Label lblWholeWidth;
    public System.Windows.Forms.Label lblRepeatColumns;
    public System.Windows.Forms.Label lblWorkWidth;
    public System.Windows.Forms.Label lblWholeWidthValueText;
    public System.Windows.Forms.Label lblWorkWidthValueText;
    private Controls.IntEditBox edRepeatColumns;
    private System.Windows.Forms.Label lblColumnSubHeaderNumbers;
    private System.Windows.Forms.ComboBox cbColumnSubHeaderNumbers;
  }
}
