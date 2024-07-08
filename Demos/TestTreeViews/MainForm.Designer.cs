﻿namespace TestTreeViews
{
  partial class MainForm
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.cbCheckBoxes = new System.Windows.Forms.CheckBox();
      this.btnBrowseDir = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.edDir = new System.Windows.Forms.TextBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbDummyModel = new System.Windows.Forms.RadioButton();
      this.rbSimpleFileModel = new System.Windows.Forms.RadioButton();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 141);
      this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(466, 40);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(100, 8);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.cbCheckBoxes);
      this.panel2.Controls.Add(this.btnBrowseDir);
      this.panel2.Controls.Add(this.label1);
      this.panel2.Controls.Add(this.edDir);
      this.panel2.Controls.Add(this.groupBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(466, 141);
      this.panel2.TabIndex = 0;
      // 
      // cbCheckBoxes
      // 
      this.cbCheckBoxes.AutoSize = true;
      this.cbCheckBoxes.Location = new System.Drawing.Point(26, 108);
      this.cbCheckBoxes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbCheckBoxes.Name = "cbCheckBoxes";
      this.cbCheckBoxes.Size = new System.Drawing.Size(86, 17);
      this.cbCheckBoxes.TabIndex = 4;
      this.cbCheckBoxes.Text = "CheckBoxes";
      this.cbCheckBoxes.UseVisualStyleBackColor = true;
      // 
      // btnBrowseDir
      // 
      this.btnBrowseDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowseDir.Location = new System.Drawing.Point(371, 59);
      this.btnBrowseDir.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnBrowseDir.Name = "btnBrowseDir";
      this.btnBrowseDir.Size = new System.Drawing.Size(88, 24);
      this.btnBrowseDir.TabIndex = 3;
      this.btnBrowseDir.Text = "Обзор";
      this.btnBrowseDir.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(175, 19);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Каталог";
      // 
      // edDir
      // 
      this.edDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edDir.Location = new System.Drawing.Point(177, 35);
      this.edDir.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edDir.Name = "edDir";
      this.edDir.Size = new System.Drawing.Size(282, 20);
      this.edDir.TabIndex = 2;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbDummyModel);
      this.groupBox1.Controls.Add(this.rbSimpleFileModel);
      this.groupBox1.Location = new System.Drawing.Point(9, 10);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Size = new System.Drawing.Size(150, 72);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Модель";
      // 
      // rbDummyModel
      // 
      this.rbDummyModel.AutoSize = true;
      this.rbDummyModel.Location = new System.Drawing.Point(16, 44);
      this.rbDummyModel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbDummyModel.Name = "rbDummyModel";
      this.rbDummyModel.Size = new System.Drawing.Size(89, 17);
      this.rbDummyModel.TabIndex = 1;
      this.rbDummyModel.TabStop = true;
      this.rbDummyModel.Text = "DummyModel";
      this.rbDummyModel.UseVisualStyleBackColor = true;
      // 
      // rbSimpleFileModel
      // 
      this.rbSimpleFileModel.AutoSize = true;
      this.rbSimpleFileModel.Location = new System.Drawing.Point(16, 22);
      this.rbSimpleFileModel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbSimpleFileModel.Name = "rbSimpleFileModel";
      this.rbSimpleFileModel.Size = new System.Drawing.Size(101, 17);
      this.rbSimpleFileModel.TabIndex = 0;
      this.rbSimpleFileModel.TabStop = true;
      this.rbSimpleFileModel.Text = "SimpleFileModel";
      this.rbSimpleFileModel.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(466, 181);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "MainForm";
      this.Text = "TestTreeViews";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.TextBox edDir;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbDummyModel;
    private System.Windows.Forms.RadioButton rbSimpleFileModel;
    private System.Windows.Forms.CheckBox cbCheckBoxes;
    private System.Windows.Forms.Button btnBrowseDir;
    private System.Windows.Forms.Label label1;
  }
}