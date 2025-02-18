namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupAppearance
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRDataViewPageSetupAppearance));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpPageBreak = new System.Windows.Forms.GroupBox();
      this.cbIgnoreWith = new System.Windows.Forms.CheckBox();
      this.grpAppearance = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.edTextFalse = new System.Windows.Forms.TextBox();
      this.lblTextTrue = new System.Windows.Forms.Label();
      this.lblTextFalse = new System.Windows.Forms.Label();
      this.edTextTrue = new System.Windows.Forms.TextBox();
      this.btnSelText = new System.Windows.Forms.Button();
      this.lblCellParams = new System.Windows.Forms.Label();
      this.btnCellParams = new System.Windows.Forms.Button();
      this.cbBoolMode = new System.Windows.Forms.ComboBox();
      this.lblBoolMode = new System.Windows.Forms.Label();
      this.cbColorStyle = new System.Windows.Forms.ComboBox();
      this.lblColorStyle = new System.Windows.Forms.Label();
      this.cbBorderStyle = new System.Windows.Forms.ComboBox();
      this.lblBorderStyle = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpPageBreak.SuspendLayout();
      this.grpAppearance.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpPageBreak);
      this.MainPanel.Controls.Add(this.grpAppearance);
      this.MainPanel.Name = "MainPanel";
      // 
      // grpPageBreak
      // 
      resources.ApplyResources(this.grpPageBreak, "grpPageBreak");
      this.grpPageBreak.Controls.Add(this.cbIgnoreWith);
      this.grpPageBreak.Name = "grpPageBreak";
      this.grpPageBreak.TabStop = false;
      // 
      // cbIgnoreWith
      // 
      resources.ApplyResources(this.cbIgnoreWith, "cbIgnoreWith");
      this.cbIgnoreWith.Name = "cbIgnoreWith";
      this.cbIgnoreWith.UseVisualStyleBackColor = true;
      // 
      // grpAppearance
      // 
      resources.ApplyResources(this.grpAppearance, "grpAppearance");
      this.grpAppearance.Controls.Add(this.tableLayoutPanel1);
      this.grpAppearance.Controls.Add(this.lblCellParams);
      this.grpAppearance.Controls.Add(this.btnCellParams);
      this.grpAppearance.Controls.Add(this.cbBoolMode);
      this.grpAppearance.Controls.Add(this.lblBoolMode);
      this.grpAppearance.Controls.Add(this.cbColorStyle);
      this.grpAppearance.Controls.Add(this.lblColorStyle);
      this.grpAppearance.Controls.Add(this.cbBorderStyle);
      this.grpAppearance.Controls.Add(this.lblBorderStyle);
      this.grpAppearance.Name = "grpAppearance";
      this.grpAppearance.TabStop = false;
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.edTextFalse, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblTextTrue, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblTextFalse, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.edTextTrue, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnSelText, 4, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // edTextFalse
      // 
      resources.ApplyResources(this.edTextFalse, "edTextFalse");
      this.edTextFalse.Name = "edTextFalse";
      // 
      // lblTextTrue
      // 
      resources.ApplyResources(this.lblTextTrue, "lblTextTrue");
      this.lblTextTrue.Name = "lblTextTrue";
      // 
      // lblTextFalse
      // 
      resources.ApplyResources(this.lblTextFalse, "lblTextFalse");
      this.lblTextFalse.Name = "lblTextFalse";
      // 
      // edTextTrue
      // 
      resources.ApplyResources(this.edTextTrue, "edTextTrue");
      this.edTextTrue.Name = "edTextTrue";
      // 
      // btnSelText
      // 
      resources.ApplyResources(this.btnSelText, "btnSelText");
      this.btnSelText.Name = "btnSelText";
      this.btnSelText.UseVisualStyleBackColor = true;
      // 
      // lblCellParams
      // 
      resources.ApplyResources(this.lblCellParams, "lblCellParams");
      this.lblCellParams.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCellParams.Name = "lblCellParams";
      this.lblCellParams.UseMnemonic = false;
      // 
      // btnCellParams
      // 
      resources.ApplyResources(this.btnCellParams, "btnCellParams");
      this.btnCellParams.Name = "btnCellParams";
      this.btnCellParams.UseVisualStyleBackColor = true;
      // 
      // cbBoolMode
      // 
      resources.ApplyResources(this.cbBoolMode, "cbBoolMode");
      this.cbBoolMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbBoolMode.FormattingEnabled = true;
      this.cbBoolMode.Items.AddRange(new object[] {
            resources.GetString("cbBoolMode.Items"),
            resources.GetString("cbBoolMode.Items1"),
            resources.GetString("cbBoolMode.Items2")});
      this.cbBoolMode.Name = "cbBoolMode";
      // 
      // lblBoolMode
      // 
      resources.ApplyResources(this.lblBoolMode, "lblBoolMode");
      this.lblBoolMode.Name = "lblBoolMode";
      // 
      // cbColorStyle
      // 
      resources.ApplyResources(this.cbColorStyle, "cbColorStyle");
      this.cbColorStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColorStyle.FormattingEnabled = true;
      this.cbColorStyle.Items.AddRange(new object[] {
            resources.GetString("cbColorStyle.Items"),
            resources.GetString("cbColorStyle.Items1"),
            resources.GetString("cbColorStyle.Items2")});
      this.cbColorStyle.Name = "cbColorStyle";
      // 
      // lblColorStyle
      // 
      resources.ApplyResources(this.lblColorStyle, "lblColorStyle");
      this.lblColorStyle.Name = "lblColorStyle";
      // 
      // cbBorderStyle
      // 
      resources.ApplyResources(this.cbBorderStyle, "cbBorderStyle");
      this.cbBorderStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbBorderStyle.FormattingEnabled = true;
      this.cbBorderStyle.Items.AddRange(new object[] {
            resources.GetString("cbBorderStyle.Items"),
            resources.GetString("cbBorderStyle.Items1"),
            resources.GetString("cbBorderStyle.Items2"),
            resources.GetString("cbBorderStyle.Items3")});
      this.cbBorderStyle.Name = "cbBorderStyle";
      // 
      // lblBorderStyle
      // 
      resources.ApplyResources(this.lblBorderStyle, "lblBorderStyle");
      this.lblBorderStyle.Name = "lblBorderStyle";
      // 
      // BRDataViewPageSetupAppearance
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupAppearance";
      this.MainPanel.ResumeLayout(false);
      this.grpPageBreak.ResumeLayout(false);
      this.grpPageBreak.PerformLayout();
      this.grpAppearance.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpPageBreak;
    private System.Windows.Forms.CheckBox cbIgnoreWith;
    public System.Windows.Forms.GroupBox grpAppearance;
    public System.Windows.Forms.ComboBox cbColorStyle;
    public System.Windows.Forms.Label lblColorStyle;
    public System.Windows.Forms.ComboBox cbBorderStyle;
    public System.Windows.Forms.Label lblBorderStyle;
    private System.Windows.Forms.ComboBox cbBoolMode;
    private System.Windows.Forms.Label lblBoolMode;
    private System.Windows.Forms.Button btnCellParams;
    private System.Windows.Forms.Label lblCellParams;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TextBox edTextFalse;
    private System.Windows.Forms.Label lblTextTrue;
    private System.Windows.Forms.Label lblTextFalse;
    private System.Windows.Forms.TextBox edTextTrue;
    private System.Windows.Forms.Button btnSelText;
  }
}
