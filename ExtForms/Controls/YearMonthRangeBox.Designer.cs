namespace FreeLibSet.Controls
{
  partial class YearMonthRangeBox
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(YearMonthRangeBox));
      this.lblFrom = new System.Windows.Forms.Label();
      this.cbMonth1 = new System.Windows.Forms.ComboBox();
      this.edYear = new FreeLibSet.Controls.Int32EditBox();
      this.cbMonth2 = new System.Windows.Forms.ComboBox();
      this.lblTill = new System.Windows.Forms.Label();
      this.lblYear = new System.Windows.Forms.Label();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblFrom
      // 
      resources.ApplyResources(this.lblFrom, "lblFrom");
      this.lblFrom.Name = "lblFrom";
      // 
      // cbMonth1
      // 
      resources.ApplyResources(this.cbMonth1, "cbMonth1");
      this.cbMonth1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth1.FormattingEnabled = true;
      this.cbMonth1.Name = "cbMonth1";
      // 
      // edYear
      // 
      resources.ApplyResources(this.edYear, "edYear");
      this.edYear.Increment = 1;
      this.edYear.Maximum = 9999;
      this.edYear.Minimum = 1001;
      this.edYear.Name = "edYear";
      // 
      // cbMonth2
      // 
      resources.ApplyResources(this.cbMonth2, "cbMonth2");
      this.cbMonth2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth2.FormattingEnabled = true;
      this.cbMonth2.Name = "cbMonth2";
      // 
      // lblTill
      // 
      resources.ApplyResources(this.lblTill, "lblTill");
      this.lblTill.Name = "lblTill";
      // 
      // lblYear
      // 
      resources.ApplyResources(this.lblYear, "lblYear");
      this.lblYear.Name = "lblYear";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.lblFrom, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblYear, 5, 0);
      this.tableLayoutPanel1.Controls.Add(this.cbMonth1, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.edYear, 4, 0);
      this.tableLayoutPanel1.Controls.Add(this.cbMonth2, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblTill, 2, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // YearMonthRangeBox
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "YearMonthRangeBox";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label lblFrom;
    internal System.Windows.Forms.ComboBox cbMonth1;
    internal FreeLibSet.Controls.Int32EditBox edYear;
    internal System.Windows.Forms.ComboBox cbMonth2;
    private System.Windows.Forms.Label lblTill;
    private System.Windows.Forms.Label lblYear;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
