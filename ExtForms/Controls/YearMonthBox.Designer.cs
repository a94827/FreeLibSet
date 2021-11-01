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
      this.label1 = new System.Windows.Forms.Label();
      this.cbMonth = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edYear = new FreeLibSet.Controls.IntEditBox();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Месяц";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbMonth
      // 
      this.cbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth.FormattingEnabled = true;
      this.cbMonth.Location = new System.Drawing.Point(76, 0);
      this.cbMonth.Name = "cbMonth";
      this.cbMonth.Size = new System.Drawing.Size(121, 21);
      this.cbMonth.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(203, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 21);
      this.label2.TabIndex = 2;
      this.label2.Text = "Год";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edYear
      // 
      this.edYear.Location = new System.Drawing.Point(266, 0);
      this.edYear.Increment = 1;
      this.edYear.Maximum = 9999;
      this.edYear.Minimum = 1001;
      this.edYear.Name = "edYear";
      this.edYear.Size = new System.Drawing.Size(78, 20);
      this.edYear.TabIndex = 3;
      // 
      // YearMonthBox
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.Controls.Add(this.edYear);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cbMonth);
      this.Controls.Add(this.label1);
      this.Name = "YearMonthBox";
      this.Size = new System.Drawing.Size(345, 21);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    internal System.Windows.Forms.ComboBox cbMonth;
    private System.Windows.Forms.Label label2;
    internal FreeLibSet.Controls.IntEditBox edYear;
  }
}
