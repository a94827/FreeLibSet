namespace FreeLibSet.Forms.Docs
{
  partial class EditDocTypePermissionForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditDocTypePermissionForm));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpMode = new FreeLibSet.Controls.RadioGroupBox();
      this.grpDocTypes = new System.Windows.Forms.GroupBox();
      this.lblDocType = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbOneType = new System.Windows.Forms.RadioButton();
      this.rbAllTypes = new System.Windows.Forms.RadioButton();
      this.cbDocType = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.grpDocTypes.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpMode);
      this.MainPanel.Controls.Add(this.grpDocTypes);
      this.MainPanel.Name = "MainPanel";
      // 
      // grpMode
      // 
      resources.ApplyResources(this.grpMode, "grpMode");
      this.grpMode.Items = new string[0];
      this.grpMode.Name = "grpMode";
      this.grpMode.TabStop = false;
      // 
      // grpDocTypes
      // 
      resources.ApplyResources(this.grpDocTypes, "grpDocTypes");
      this.grpDocTypes.Controls.Add(this.lblDocType);
      this.grpDocTypes.Controls.Add(this.panel1);
      this.grpDocTypes.Controls.Add(this.cbDocType);
      this.grpDocTypes.Name = "grpDocTypes";
      this.grpDocTypes.TabStop = false;
      // 
      // lblDocType
      // 
      resources.ApplyResources(this.lblDocType, "lblDocType");
      this.lblDocType.Name = "lblDocType";
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.rbOneType);
      this.panel1.Controls.Add(this.rbAllTypes);
      this.panel1.Name = "panel1";
      // 
      // rbOneType
      // 
      resources.ApplyResources(this.rbOneType, "rbOneType");
      this.rbOneType.Name = "rbOneType";
      this.rbOneType.TabStop = true;
      this.rbOneType.UseVisualStyleBackColor = true;
      // 
      // rbAllTypes
      // 
      resources.ApplyResources(this.rbAllTypes, "rbAllTypes");
      this.rbAllTypes.Name = "rbAllTypes";
      this.rbAllTypes.TabStop = true;
      this.rbAllTypes.UseVisualStyleBackColor = true;
      // 
      // cbDocType
      // 
      resources.ApplyResources(this.cbDocType, "cbDocType");
      this.cbDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDocType.FormattingEnabled = true;
      this.cbDocType.Name = "cbDocType";
      // 
      // EditDocTypePermissionForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "EditDocTypePermissionForm";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.grpDocTypes.ResumeLayout(false);
      this.grpDocTypes.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpDocTypes;
    private FreeLibSet.Controls.RadioGroupBox grpMode;
    private System.Windows.Forms.ComboBox cbDocType;
    public System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbOneType;
    private System.Windows.Forms.RadioButton rbAllTypes;
    private System.Windows.Forms.Label lblDocType;

  }
}
