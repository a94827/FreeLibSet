namespace FreeLibSet.Controls
{
  partial class YearMonthBox
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YearMonthBox));
      this.lblMonth = new System.Windows.Forms.Label();
      this.cbMonth = new System.Windows.Forms.ComboBox();
      this.lblYear = new System.Windows.Forms.Label();
      this.edYear = new FreeLibSet.Controls.IntEditBox();
      this.SuspendLayout();
      // 
      // lblMonth
      // 
      resources.ApplyResources(this.lblMonth, "lblMonth");
      this.lblMonth.Name = "lblMonth";
      // 
      // cbMonth
      // 
      resources.ApplyResources(this.cbMonth, "cbMonth");
      this.cbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth.FormattingEnabled = true;
      this.cbMonth.Name = "cbMonth";
      // 
      // lblYear
      // 
      resources.ApplyResources(this.lblYear, "lblYear");
      this.lblYear.Name = "lblYear";
      // 
      // edYear
      // 
      resources.ApplyResources(this.edYear, "edYear");
      this.edYear.Increment = 1;
      this.edYear.Maximum = 9999;
      this.edYear.Minimum = 1001;
      this.edYear.Name = "edYear";
      // 
      // YearMonthBox
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.edYear);
      this.Controls.Add(this.lblYear);
      this.Controls.Add(this.cbMonth);
      this.Controls.Add(this.lblMonth);
      this.Name = "YearMonthBox";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblMonth;
    internal System.Windows.Forms.ComboBox cbMonth;
    private System.Windows.Forms.Label lblYear;
    internal FreeLibSet.Controls.IntEditBox edYear;
  }
}
