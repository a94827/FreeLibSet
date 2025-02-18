﻿namespace WinFormsDemo.EFPDateRangeBoxDemo
{
  partial class DateRangeBoxForm
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.panel1 = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.edDateOrRange = new FreeLibSet.Controls.UserMaskedComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edDateRange = new FreeLibSet.Controls.DateRangeBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.label7 = new System.Windows.Forms.Label();
      this.cbCanBeEmptyMode = new System.Windows.Forms.ComboBox();
      this.cbCanBeHalfEmpty = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edMinimum = new FreeLibSet.Controls.DateTimeBox();
      this.edMaximum = new FreeLibSet.Controls.DateTimeBox();
      this.label4 = new System.Windows.Forms.Label();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 281);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(679, 40);
      this.panel1.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.edDateOrRange);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.edDateRange);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(679, 117);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Controls";
      // 
      // edDateOrRange
      // 
      this.edDateOrRange.ClearButtonEnabled = false;
      this.edDateOrRange.Culture = new System.Globalization.CultureInfo("ru-RU");
      this.edDateOrRange.Location = new System.Drawing.Point(195, 76);
      this.edDateOrRange.Name = "edDateOrRange";
      this.edDateOrRange.Size = new System.Drawing.Size(467, 22);
      this.edDateOrRange.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(18, 76);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(171, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "EFPDateOrRangeBox";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDateRange
      // 
      this.edDateRange.Location = new System.Drawing.Point(195, 22);
      this.edDateRange.Margin = new System.Windows.Forms.Padding(4);
      this.edDateRange.Name = "edDateRange";
      this.edDateRange.Size = new System.Drawing.Size(467, 45);
      this.edDateRange.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(18, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(171, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "EFPDateRangeBox";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.edMaximum);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.edMinimum);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.cbCanBeHalfEmpty);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.cbCanBeEmptyMode);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 117);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(679, 164);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Controlling properties";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(21, 31);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(168, 24);
      this.label7.TabIndex = 6;
      this.label7.Text = "CanBeEmptyMode";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbCanBeEmptyMode
      // 
      this.cbCanBeEmptyMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCanBeEmptyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCanBeEmptyMode.FormattingEnabled = true;
      this.cbCanBeEmptyMode.Location = new System.Drawing.Point(195, 31);
      this.cbCanBeEmptyMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbCanBeEmptyMode.Name = "cbCanBeEmptyMode";
      this.cbCanBeEmptyMode.Size = new System.Drawing.Size(467, 24);
      this.cbCanBeEmptyMode.TabIndex = 7;
      // 
      // cbCanBeHalfEmpty
      // 
      this.cbCanBeHalfEmpty.AutoSize = true;
      this.cbCanBeHalfEmpty.Location = new System.Drawing.Point(28, 70);
      this.cbCanBeHalfEmpty.Name = "cbCanBeHalfEmpty";
      this.cbCanBeHalfEmpty.Size = new System.Drawing.Size(271, 21);
      this.cbCanBeHalfEmpty.TabIndex = 8;
      this.cbCanBeHalfEmpty.Text = "CabBeHalfEmpty (EFPDateRangeBox)";
      this.cbCanBeHalfEmpty.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(18, 104);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(171, 22);
      this.label3.TabIndex = 9;
      this.label3.Text = "Minimum";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edMinimum
      // 
      this.edMinimum.Location = new System.Drawing.Point(195, 104);
      this.edMinimum.Name = "edMinimum";
      this.edMinimum.Size = new System.Drawing.Size(182, 22);
      this.edMinimum.TabIndex = 10;
      // 
      // edMaximum
      // 
      this.edMaximum.Location = new System.Drawing.Point(195, 132);
      this.edMaximum.Name = "edMaximum";
      this.edMaximum.Size = new System.Drawing.Size(182, 22);
      this.edMaximum.TabIndex = 12;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(18, 132);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(171, 22);
      this.label4.TabIndex = 11;
      this.label4.Text = "Maximum";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // DateRangeBoxForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(679, 321);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DateRangeBoxForm";
      this.Text = "DateRangeBoxForm";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private FreeLibSet.Controls.UserMaskedComboBox edDateOrRange;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.DateRangeBox edDateRange;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox cbCanBeEmptyMode;
    private System.Windows.Forms.CheckBox cbCanBeHalfEmpty;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private FreeLibSet.Controls.DateTimeBox edMaximum;
    private System.Windows.Forms.Label label4;
    private FreeLibSet.Controls.DateTimeBox edMinimum;
    private System.Windows.Forms.Label label3;
  }
}