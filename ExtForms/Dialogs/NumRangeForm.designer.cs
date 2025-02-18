namespace FreeLibSet.Forms
{
  partial class NumRangeForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NumRangeForm));
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheGroup = new System.Windows.Forms.GroupBox();
      this.lblRange = new System.Windows.Forms.Label();
      this.btn2eq1 = new System.Windows.Forms.Button();
      this.lblMaximum = new System.Windows.Forms.Label();
      this.lblMinimum = new System.Windows.Forms.Label();
      this.btnNo = new System.Windows.Forms.Button();
      this.TheGroup.SuspendLayout();
      this.SuspendLayout();
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
      // TheGroup
      // 
      resources.ApplyResources(this.TheGroup, "TheGroup");
      this.TheGroup.Controls.Add(this.lblRange);
      this.TheGroup.Controls.Add(this.btn2eq1);
      this.TheGroup.Controls.Add(this.lblMaximum);
      this.TheGroup.Controls.Add(this.lblMinimum);
      this.TheGroup.Name = "TheGroup";
      this.TheGroup.TabStop = false;
      // 
      // lblRange
      // 
      resources.ApplyResources(this.lblRange, "lblRange");
      this.lblRange.Name = "lblRange";
      // 
      // btn2eq1
      // 
      resources.ApplyResources(this.btn2eq1, "btn2eq1");
      this.btn2eq1.Name = "btn2eq1";
      this.btn2eq1.UseVisualStyleBackColor = true;
      // 
      // lblMaximum
      // 
      resources.ApplyResources(this.lblMaximum, "lblMaximum");
      this.lblMaximum.Name = "lblMaximum";
      // 
      // lblMinimum
      // 
      resources.ApplyResources(this.lblMinimum, "lblMinimum");
      this.lblMinimum.Name = "lblMinimum";
      // 
      // btnNo
      // 
      resources.ApplyResources(this.btnNo, "btnNo");
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Name = "btnNo";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // NumRangeForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.btnNo);
      this.Controls.Add(this.TheGroup);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NumRangeForm";
      this.TheGroup.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.GroupBox TheGroup;
    private System.Windows.Forms.Button btn2eq1;
    internal System.Windows.Forms.Label lblMaximum;
    internal System.Windows.Forms.Label lblMinimum;
    private System.Windows.Forms.Button btnNo;
    public System.Windows.Forms.Label lblRange;
  }
}
