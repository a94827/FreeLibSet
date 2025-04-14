namespace FreeLibSet.Forms.Docs
{
  partial class UserActionsReportParamsForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserActionsReportParamsForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.grpSourceData = new System.Windows.Forms.GroupBox();
      this.cbOneType = new System.Windows.Forms.CheckBox();
      this.cbDocType = new System.Windows.Forms.ComboBox();
      this.cbUser = new FreeLibSet.Controls.UserSelComboBox();
      this.lblUser = new System.Windows.Forms.Label();
      this.grpPeriod = new System.Windows.Forms.GroupBox();
      this.btnLastDay = new System.Windows.Forms.Button();
      this.edPeriod = new FreeLibSet.Controls.DateRangeBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.grpSourceData.SuspendLayout();
      this.grpPeriod.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Name = "panel1";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.grpSourceData);
      this.panel2.Controls.Add(this.grpPeriod);
      this.panel2.Name = "panel2";
      // 
      // grpSourceData
      // 
      resources.ApplyResources(this.grpSourceData, "grpSourceData");
      this.grpSourceData.Controls.Add(this.cbOneType);
      this.grpSourceData.Controls.Add(this.cbDocType);
      this.grpSourceData.Controls.Add(this.cbUser);
      this.grpSourceData.Controls.Add(this.lblUser);
      this.grpSourceData.Name = "grpSourceData";
      this.grpSourceData.TabStop = false;
      // 
      // cbOneType
      // 
      resources.ApplyResources(this.cbOneType, "cbOneType");
      this.cbOneType.Name = "cbOneType";
      this.cbOneType.UseVisualStyleBackColor = true;
      // 
      // cbDocType
      // 
      resources.ApplyResources(this.cbDocType, "cbDocType");
      this.cbDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDocType.FormattingEnabled = true;
      this.cbDocType.Name = "cbDocType";
      // 
      // cbUser
      // 
      resources.ApplyResources(this.cbUser, "cbUser");
      this.cbUser.Name = "cbUser";
      // 
      // lblUser
      // 
      resources.ApplyResources(this.lblUser, "lblUser");
      this.lblUser.Name = "lblUser";
      // 
      // grpPeriod
      // 
      resources.ApplyResources(this.grpPeriod, "grpPeriod");
      this.grpPeriod.Controls.Add(this.btnLastDay);
      this.grpPeriod.Controls.Add(this.edPeriod);
      this.grpPeriod.Name = "grpPeriod";
      this.grpPeriod.TabStop = false;
      // 
      // btnLastDay
      // 
      resources.ApplyResources(this.btnLastDay, "btnLastDay");
      this.btnLastDay.Name = "btnLastDay";
      this.btnLastDay.UseVisualStyleBackColor = true;
      // 
      // edPeriod
      // 
      resources.ApplyResources(this.edPeriod, "edPeriod");
      this.edPeriod.Name = "edPeriod";
      // 
      // UserActionsReportParamsForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "UserActionsReportParamsForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.grpSourceData.ResumeLayout(false);
      this.grpSourceData.PerformLayout();
      this.grpPeriod.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox grpPeriod;
    private FreeLibSet.Controls.DateRangeBox edPeriod;
    private System.Windows.Forms.GroupBox grpSourceData;
    private System.Windows.Forms.CheckBox cbOneType;
    private System.Windows.Forms.ComboBox cbDocType;
    public FreeLibSet.Controls.UserSelComboBox cbUser;
    private System.Windows.Forms.Label lblUser;
    private System.Windows.Forms.Button btnLastDay;
  }
}
