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
      this.pbIcon.Location = new System.Drawing.Point(22, 41);
      this.pbIcon.Name = "pbIcon";
      this.pbIcon.Size = new System.Drawing.Size(32, 32);
      this.pbIcon.TabIndex = 0;
      this.pbIcon.TabStop = false;
      // 
      // lblTitle
      // 
      this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblTitle.Location = new System.Drawing.Point(89, 28);
      this.lblTitle.Name = "lblTitle";
      this.lblTitle.Size = new System.Drawing.Size(314, 57);
      this.lblTitle.TabIndex = 0;
      this.lblTitle.Text = "???";
      this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblVersion
      // 
      this.lblVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblVersion.Location = new System.Drawing.Point(89, 85);
      this.lblVersion.Name = "lblVersion";
      this.lblVersion.Size = new System.Drawing.Size(314, 23);
      this.lblVersion.TabIndex = 1;
      this.lblVersion.Text = "???";
      this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblNetFramework
      // 
      this.lblNetFramework.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblNetFramework.Location = new System.Drawing.Point(12, 203);
      this.lblNetFramework.Name = "lblNetFramework";
      this.lblNetFramework.Size = new System.Drawing.Size(466, 21);
      this.lblNetFramework.TabIndex = 2;
      this.lblNetFramework.Text = "???";
      this.lblNetFramework.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblCopyright
      // 
      this.lblCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCopyright.Location = new System.Drawing.Point(89, 124);
      this.lblCopyright.Name = "lblCopyright";
      this.lblCopyright.Size = new System.Drawing.Size(314, 65);
      this.lblCopyright.TabIndex = 3;
      this.lblCopyright.Text = "???";
      this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(202, 227);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 4;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnModules
      // 
      this.btnModules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnModules.Location = new System.Drawing.Point(296, 227);
      this.btnModules.Name = "btnModules";
      this.btnModules.Size = new System.Drawing.Size(88, 24);
      this.btnModules.TabIndex = 5;
      this.btnModules.Text = "Модули...";
      this.btnModules.UseVisualStyleBackColor = true;
      // 
      // btnInfo
      // 
      this.btnInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnInfo.Location = new System.Drawing.Point(390, 226);
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.Size = new System.Drawing.Size(88, 24);
      this.btnInfo.TabIndex = 6;
      this.btnInfo.Text = "Инфо...";
      this.btnInfo.UseVisualStyleBackColor = true;
      // 
      // SimpleAboutDialogForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOk;
      this.ClientSize = new System.Drawing.Size(493, 262);
      this.Controls.Add(this.btnInfo);
      this.Controls.Add(this.btnModules);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.lblCopyright);
      this.Controls.Add(this.lblNetFramework);
      this.Controls.Add(this.lblVersion);
      this.Controls.Add(this.lblTitle);
      this.Controls.Add(this.pbIcon);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SimpleAboutDialogForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "О программе";
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
