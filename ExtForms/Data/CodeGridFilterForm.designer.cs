namespace FreeLibSet.Forms.Data
{
  partial class CodeGridFilterForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeGridFilterForm));
      this.grpMode = new System.Windows.Forms.GroupBox();
      this.rbExclude = new System.Windows.Forms.RadioButton();
      this.rbInclude = new System.Windows.Forms.RadioButton();
      this.rbNoFilter = new System.Windows.Forms.RadioButton();
      this.CodesGroupBox = new System.Windows.Forms.GroupBox();
      this.lblCodes = new System.Windows.Forms.Label();
      this.cbEmpty = new System.Windows.Forms.CheckBox();
      this.edCodes = new FreeLibSet.Controls.UserTextComboBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpMode.SuspendLayout();
      this.CodesGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpMode
      // 
      this.grpMode.AccessibleDescription = null;
      this.grpMode.AccessibleName = null;
      resources.ApplyResources(this.grpMode, "grpMode");
      this.grpMode.BackgroundImage = null;
      this.grpMode.Controls.Add(this.rbExclude);
      this.grpMode.Controls.Add(this.rbInclude);
      this.grpMode.Controls.Add(this.rbNoFilter);
      this.grpMode.Font = null;
      this.grpMode.Name = "grpMode";
      this.grpMode.TabStop = false;
      // 
      // rbExclude
      // 
      this.rbExclude.AccessibleDescription = null;
      this.rbExclude.AccessibleName = null;
      resources.ApplyResources(this.rbExclude, "rbExclude");
      this.rbExclude.BackgroundImage = null;
      this.rbExclude.Font = null;
      this.rbExclude.Name = "rbExclude";
      this.rbExclude.TabStop = true;
      this.rbExclude.UseVisualStyleBackColor = true;
      // 
      // rbInclude
      // 
      this.rbInclude.AccessibleDescription = null;
      this.rbInclude.AccessibleName = null;
      resources.ApplyResources(this.rbInclude, "rbInclude");
      this.rbInclude.BackgroundImage = null;
      this.rbInclude.Font = null;
      this.rbInclude.Name = "rbInclude";
      this.rbInclude.TabStop = true;
      this.rbInclude.UseVisualStyleBackColor = true;
      // 
      // rbNoFilter
      // 
      this.rbNoFilter.AccessibleDescription = null;
      this.rbNoFilter.AccessibleName = null;
      resources.ApplyResources(this.rbNoFilter, "rbNoFilter");
      this.rbNoFilter.BackgroundImage = null;
      this.rbNoFilter.Font = null;
      this.rbNoFilter.Name = "rbNoFilter";
      this.rbNoFilter.TabStop = true;
      this.rbNoFilter.UseVisualStyleBackColor = true;
      // 
      // CodesGroupBox
      // 
      this.CodesGroupBox.AccessibleDescription = null;
      this.CodesGroupBox.AccessibleName = null;
      resources.ApplyResources(this.CodesGroupBox, "CodesGroupBox");
      this.CodesGroupBox.BackgroundImage = null;
      this.CodesGroupBox.Controls.Add(this.lblCodes);
      this.CodesGroupBox.Controls.Add(this.cbEmpty);
      this.CodesGroupBox.Controls.Add(this.edCodes);
      this.CodesGroupBox.Font = null;
      this.CodesGroupBox.Name = "CodesGroupBox";
      this.CodesGroupBox.TabStop = false;
      // 
      // lblCodes
      // 
      this.lblCodes.AccessibleDescription = null;
      this.lblCodes.AccessibleName = null;
      resources.ApplyResources(this.lblCodes, "lblCodes");
      this.lblCodes.Font = null;
      this.lblCodes.Name = "lblCodes";
      // 
      // cbEmpty
      // 
      this.cbEmpty.AccessibleDescription = null;
      this.cbEmpty.AccessibleName = null;
      resources.ApplyResources(this.cbEmpty, "cbEmpty");
      this.cbEmpty.BackgroundImage = null;
      this.cbEmpty.Font = null;
      this.cbEmpty.Name = "cbEmpty";
      this.cbEmpty.UseVisualStyleBackColor = true;
      // 
      // edCodes
      // 
      this.edCodes.AccessibleDescription = null;
      this.edCodes.AccessibleName = null;
      resources.ApplyResources(this.edCodes, "edCodes");
      this.edCodes.ClearButtonEnabled = false;
      this.edCodes.Font = null;
      this.edCodes.Name = "edCodes";
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
      // CodeGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.CodesGroupBox);
      this.Controls.Add(this.grpMode);
      this.Font = null;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Icon = null;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CodeGridFilterForm";
      this.grpMode.ResumeLayout(false);
      this.grpMode.PerformLayout();
      this.CodesGroupBox.ResumeLayout(false);
      this.CodesGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpMode;
    private System.Windows.Forms.RadioButton rbExclude;
    private System.Windows.Forms.RadioButton rbInclude;
    private System.Windows.Forms.RadioButton rbNoFilter;
    private System.Windows.Forms.GroupBox CodesGroupBox;
    private System.Windows.Forms.CheckBox cbEmpty;
    private FreeLibSet.Controls.UserTextComboBox edCodes;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblCodes;
  }
}
