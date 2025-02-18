namespace FreeLibSet.Forms
{
  partial class SelectCompositionForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectCompositionForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.grpPresets = new System.Windows.Forms.GroupBox();
      this.cbParamSet = new FreeLibSet.Controls.ParamSetComboBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.grpInfo = new System.Windows.Forms.GroupBox();
      this.lblInfo = new FreeLibSet.Controls.InfoLabel();
      this.pbPreview = new System.Windows.Forms.PictureBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnXml = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.grpPresets.SuspendLayout();
      this.panel2.SuspendLayout();
      this.grpInfo.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.AccessibleDescription = null;
      this.panel1.AccessibleName = null;
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.BackgroundImage = null;
      this.panel1.Controls.Add(this.grpPresets);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Font = null;
      this.panel1.Name = "panel1";
      // 
      // grpPresets
      // 
      this.grpPresets.AccessibleDescription = null;
      this.grpPresets.AccessibleName = null;
      resources.ApplyResources(this.grpPresets, "grpPresets");
      this.grpPresets.BackgroundImage = null;
      this.grpPresets.Controls.Add(this.cbParamSet);
      this.grpPresets.Font = null;
      this.grpPresets.Name = "grpPresets";
      this.grpPresets.TabStop = false;
      // 
      // cbParamSet
      // 
      this.cbParamSet.AccessibleDescription = null;
      this.cbParamSet.AccessibleName = null;
      resources.ApplyResources(this.cbParamSet, "cbParamSet");
      this.cbParamSet.BackgroundImage = null;
      this.cbParamSet.Font = null;
      this.cbParamSet.MinimumSize = new System.Drawing.Size(200, 24);
      this.cbParamSet.Name = "cbParamSet";
      this.cbParamSet.SelectedCode = "";
      this.cbParamSet.SelectedItem = null;
      this.cbParamSet.SelectedMD5Sum = "";
      this.cbParamSet.ShowMD5 = false;
      // 
      // panel2
      // 
      this.panel2.AccessibleDescription = null;
      this.panel2.AccessibleName = null;
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.BackgroundImage = null;
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Controls.Add(this.btnOk);
      this.panel2.Font = null;
      this.panel2.Name = "panel2";
      // 
      // btnCancel
      // 
      this.btnCancel.AccessibleDescription = null;
      this.btnCancel.AccessibleName = null;
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.BackgroundImage = null;
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Font = null;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
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
      // grpInfo
      // 
      this.grpInfo.AccessibleDescription = null;
      this.grpInfo.AccessibleName = null;
      resources.ApplyResources(this.grpInfo, "grpInfo");
      this.grpInfo.BackgroundImage = null;
      this.grpInfo.Controls.Add(this.lblInfo);
      this.grpInfo.Controls.Add(this.pbPreview);
      this.grpInfo.Controls.Add(this.panel3);
      this.grpInfo.Font = null;
      this.grpInfo.Name = "grpInfo";
      this.grpInfo.TabStop = false;
      // 
      // lblInfo
      // 
      this.lblInfo.AccessibleDescription = null;
      this.lblInfo.AccessibleName = null;
      resources.ApplyResources(this.lblInfo, "lblInfo");
      this.lblInfo.BackgroundImage = null;
      this.lblInfo.Font = null;
      this.lblInfo.IconSize = FreeLibSet.Controls.MessageBoxIconSize.Large;
      this.lblInfo.Name = "lblInfo";
      // 
      // pbPreview
      // 
      this.pbPreview.AccessibleDescription = null;
      this.pbPreview.AccessibleName = null;
      resources.ApplyResources(this.pbPreview, "pbPreview");
      this.pbPreview.BackgroundImage = null;
      this.pbPreview.Font = null;
      this.pbPreview.ImageLocation = null;
      this.pbPreview.Name = "pbPreview";
      this.pbPreview.TabStop = false;
      // 
      // panel3
      // 
      this.panel3.AccessibleDescription = null;
      this.panel3.AccessibleName = null;
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.BackgroundImage = null;
      this.panel3.Controls.Add(this.btnXml);
      this.panel3.Font = null;
      this.panel3.Name = "panel3";
      // 
      // btnXml
      // 
      this.btnXml.AccessibleDescription = null;
      this.btnXml.AccessibleName = null;
      resources.ApplyResources(this.btnXml, "btnXml");
      this.btnXml.BackgroundImage = null;
      this.btnXml.Font = null;
      this.btnXml.Name = "btnXml";
      this.btnXml.UseVisualStyleBackColor = true;
      // 
      // SelectCompositionForm
      // 
      this.AcceptButton = this.btnOk;
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.grpInfo);
      this.Controls.Add(this.panel1);
      this.Font = null;
      this.Icon = null;
      this.Name = "SelectCompositionForm";
      this.panel1.ResumeLayout(false);
      this.grpPresets.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.grpInfo.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
      this.panel3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox grpPresets;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox grpInfo;
    private FreeLibSet.Controls.ParamSetComboBox cbParamSet;
    private FreeLibSet.Controls.InfoLabel lblInfo;
    private System.Windows.Forms.PictureBox pbPreview;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnXml;
  }
}
