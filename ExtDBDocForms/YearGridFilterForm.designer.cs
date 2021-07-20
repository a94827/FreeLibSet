namespace AgeyevAV.ExtForms.Docs
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.edYear = new AgeyevAV.ExtForms.ExtNumericUpDown();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbFilter = new System.Windows.Forms.RadioButton();
      this.rbNoFilter = new System.Windows.Forms.RadioButton();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.edYear);
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(202, 91);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Фильтр по году";
      // 
      // edYear
      // 
      this.edYear.Location = new System.Drawing.Point(115, 53);
      this.edYear.Maximum = new decimal(new int[] {
            2099,
            0,
            0,
            0});
      this.edYear.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
      this.edYear.Name = "edYear";
      this.edYear.Size = new System.Drawing.Size(73, 20);
      this.edYear.TabIndex = 1;
      this.edYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.edYear.Value = new decimal(new int[] {
            1900,
            0,
            0,
            0});
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.rbFilter);
      this.panel1.Controls.Add(this.rbNoFilter);
      this.panel1.Location = new System.Drawing.Point(6, 19);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(103, 69);
      this.panel1.TabIndex = 0;
      // 
      // rbFilter
      // 
      this.rbFilter.AutoSize = true;
      this.rbFilter.Location = new System.Drawing.Point(8, 34);
      this.rbFilter.Name = "rbFilter";
      this.rbFilter.Size = new System.Drawing.Size(43, 17);
      this.rbFilter.TabIndex = 1;
      this.rbFilter.TabStop = true;
      this.rbFilter.Text = "&Год";
      this.rbFilter.UseVisualStyleBackColor = true;
      // 
      // rbNoFilter
      // 
      this.rbNoFilter.AutoSize = true;
      this.rbNoFilter.Location = new System.Drawing.Point(8, 11);
      this.rbNoFilter.Name = "rbNoFilter";
      this.rbNoFilter.Size = new System.Drawing.Size(90, 17);
      this.rbNoFilter.TabIndex = 0;
      this.rbNoFilter.TabStop = true;
      this.rbNoFilter.Text = "&Нет фильтра";
      this.rbNoFilter.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(220, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(220, 42);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // YearGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(314, 112);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "YearGridFilterForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.groupBox1.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private AgeyevAV.ExtForms.ExtNumericUpDown edYear;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbFilter;
    private System.Windows.Forms.RadioButton rbNoFilter;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
  }
}