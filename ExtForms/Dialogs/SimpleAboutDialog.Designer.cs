namespace FreeLibSet.Forms
{
  partial class SimpleAboutDialogForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleAboutDialogForm));
      this.pbIcon = new System.Windows.Forms.PictureBox();
      this.lblTitle = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.lblNetFramework = new System.Windows.Forms.Label();
      this.lblCopyright = new System.Windows.Forms.Label();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnModules = new System.Windows.Forms.Button();
      this.btnInfo = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
      this.SuspendLayout();
      // 
      // pbIcon
      // 
      this.pbIcon.AccessibleDescription = null;
      this.pbIcon.AccessibleName = null;
      resources.ApplyResources(this.pbIcon, "pbIcon");
      this.pbIcon.BackgroundImage = null;
      this.pbIcon.Font = null;
      this.pbIcon.ImageLocation = null;
      this.pbIcon.Name = "pbIcon";
      this.pbIcon.TabStop = false;
      // 
      // lblTitle
      // 
      this.lblTitle.AccessibleDescription = null;
      this.lblTitle.AccessibleName = null;
      resources.ApplyResources(this.lblTitle, "lblTitle");
      this.lblTitle.Name = "lblTitle";
      // 
      // lblVersion
      // 
      this.lblVersion.AccessibleDescription = null;
      this.lblVersion.AccessibleName = null;
      resources.ApplyResources(this.lblVersion, "lblVersion");
      this.lblVersion.Font = null;
      this.lblVersion.Name = "lblVersion";
      // 
      // lblNetFramework
      // 
      this.lblNetFramework.AccessibleDescription = null;
      this.lblNetFramework.AccessibleName = null;
      resources.ApplyResources(this.lblNetFramework, "lblNetFramework");
      this.lblNetFramework.Font = null;
      this.lblNetFramework.Name = "lblNetFramework";
      // 
      // lblCopyright
      // 
      this.lblCopyright.AccessibleDescription = null;
      this.lblCopyright.AccessibleName = null;
      resources.ApplyResources(this.lblCopyright, "lblCopyright");
      this.lblCopyright.Font = null;
      this.lblCopyright.Name = "lblCopyright";
      // 
      // btnOk
      // 
      this.btnOk.AccessibleDescription = null;
      this.btnOk.AccessibleName = null;
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.BackgroundImage = null;
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Font = null;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnModules
      // 
      this.btnModules.AccessibleDescription = null;
      this.btnModules.AccessibleName = null;
      resources.ApplyResources(this.btnModules, "btnModules");
      this.btnModules.BackgroundImage = null;
      this.btnModules.Font = null;
      this.btnModules.Name = "btnModules";
      this.btnModules.UseVisualStyleBackColor = true;
      // 
      // btnInfo
      // 
      this.btnInfo.AccessibleDescription = null;
      this.btnInfo.AccessibleName = null;
      resources.ApplyResources(this.btnInfo, "btnInfo");
      this.btnInfo.BackgroundImage = null;
      this.btnInfo.Font = null;
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.UseVisualStyleBackColor = true;
      // 
      // SimpleAboutDialogForm
      // 
      this.AcceptButton = this.btnOk;
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnOk;
      this.Controls.Add(this.btnInfo);
      this.Controls.Add(this.btnModules);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.lblCopyright);
      this.Controls.Add(this.lblNetFramework);
      this.Controls.Add(this.lblVersion);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.pbIcon);
      this.Font = null;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = null;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SimpleAboutDialogForm";
      ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.PictureBox pbIcon;
    public System.Windows.Forms.Label lblTitle;
    public System.Windows.Forms.Label lblVersion;
    public System.Windows.Forms.Label lblNetFramework;
    public System.Windows.Forms.Label lblCopyright;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnModules;
    private System.Windows.Forms.Button btnInfo;
  }
}
