namespace FreeLibSet.Forms
{
  partial class DateRangeForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateRangeForm));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.edRange = new FreeLibSet.Controls.DateRangeBox();
      this.MainLabel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // edRange
      // 
      resources.ApplyResources(this.edRange, "edRange");
      this.edRange.Name = "edRange";
      // 
      // MainLabel
      // 
      resources.ApplyResources(this.MainLabel, "MainLabel");
      this.MainLabel.Name = "MainLabel";
      // 
      // DateRangeForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.MainLabel);
      this.Controls.Add(this.edRange);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DateRangeForm";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private FreeLibSet.Controls.DateRangeBox edRange;
    public System.Windows.Forms.Label MainLabel;
  }
}
