namespace AgeyevAV.ExtForms
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
      this.label1 = new System.Windows.Forms.Label();
      this.cbMonth1 = new System.Windows.Forms.ComboBox();
      this.edYear = new AgeyevAV.ExtForms.ExtNumericUpDown();
      this.cbMonth2 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(19, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "с";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbMonth1
      // 
      this.cbMonth1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth1.FormattingEnabled = true;
      this.cbMonth1.Location = new System.Drawing.Point(25, 0);
      this.cbMonth1.Name = "cbMonth1";
      this.cbMonth1.Size = new System.Drawing.Size(121, 21);
      this.cbMonth1.TabIndex = 1;
      // 
      // edYear
      // 
      this.edYear.Location = new System.Drawing.Point(306, 0);
      this.edYear.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
      this.edYear.Minimum = new decimal(new int[] {
            1001,
            0,
            0,
            0});
      this.edYear.Name = "edYear";
      this.edYear.Size = new System.Drawing.Size(65, 20);
      this.edYear.TabIndex = 5;
      this.edYear.Value = new decimal(new int[] {
            1001,
            0,
            0,
            0});
      // 
      // cbMonth2
      // 
      this.cbMonth2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth2.FormattingEnabled = true;
      this.cbMonth2.Location = new System.Drawing.Point(179, 0);
      this.cbMonth2.Name = "cbMonth2";
      this.cbMonth2.Size = new System.Drawing.Size(121, 21);
      this.cbMonth2.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(152, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(21, 21);
      this.label3.TabIndex = 2;
      this.label3.Text = "по";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(377, -1);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(44, 21);
      this.label2.TabIndex = 4;
      this.label2.Text = "года";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // YearMonthRangeBox
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cbMonth2);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.edYear);
      this.Controls.Add(this.cbMonth1);
      this.Controls.Add(this.label1);
      this.Name = "YearMonthRangeBox";
      this.Size = new System.Drawing.Size(424, 21);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    internal System.Windows.Forms.ComboBox cbMonth1;
    internal AgeyevAV.ExtForms.ExtNumericUpDown edYear;
    internal System.Windows.Forms.ComboBox cbMonth2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
  }
}
