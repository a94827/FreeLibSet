namespace FreeLibSet.Forms.Data
{
  partial class YearGridFilterForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YearGridFilterForm));
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.edYear = new FreeLibSet.Controls.Int32EditBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbFilter = new System.Windows.Forms.RadioButton();
      this.rbNoFilter = new System.Windows.Forms.RadioButton();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpMain.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpMain
      // 
      this.grpMain.AccessibleDescription = null;
      this.grpMain.AccessibleName = null;
      resources.ApplyResources(this.grpMain, "grpMain");
      this.grpMain.BackgroundImage = null;
      this.grpMain.Controls.Add(this.edYear);
      this.grpMain.Controls.Add(this.panel1);
      this.grpMain.Font = null;
      this.grpMain.Name = "grpMain";
      this.grpMain.TabStop = false;
      // 
      // edYear
      // 
      this.edYear.AccessibleDescription = null;
      this.edYear.AccessibleName = null;
      resources.ApplyResources(this.edYear, "edYear");
      this.edYear.Font = null;
      this.edYear.Name = "edYear";
      // 
      // panel1
      // 
      this.panel1.AccessibleDescription = null;
      this.panel1.AccessibleName = null;
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.BackgroundImage = null;
      this.panel1.Controls.Add(this.rbFilter);
      this.panel1.Controls.Add(this.rbNoFilter);
      this.panel1.Font = null;
      this.panel1.Name = "panel1";
      // 
      // rbFilter
      // 
      this.rbFilter.AccessibleDescription = null;
      this.rbFilter.AccessibleName = null;
      resources.ApplyResources(this.rbFilter, "rbFilter");
      this.rbFilter.BackgroundImage = null;
      this.rbFilter.Font = null;
      this.rbFilter.Name = "rbFilter";
      this.rbFilter.TabStop = true;
      this.rbFilter.UseVisualStyleBackColor = true;
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
      // YearGridFilterForm
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
      this.Controls.Add(this.grpMain);
      this.Font = null;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Icon = null;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "YearGridFilterForm";
      this.grpMain.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpMain;
    private FreeLibSet.Controls.Int32EditBox edYear;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbFilter;
    private System.Windows.Forms.RadioButton rbNoFilter;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
  }
}
