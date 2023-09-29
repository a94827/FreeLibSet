﻿namespace WizardDemo
{
  partial class TemplateForm
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
      this.tp101 = new System.Windows.Forms.TabPage();
      this.Pan101 = new System.Windows.Forms.Panel();
      this.edTest101 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tp201 = new System.Windows.Forms.TabPage();
      this.Pan201 = new System.Windows.Forms.Panel();
      this.cbInfoIconSize201 = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cbInfoIcon201 = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbInfoText201 = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbGroupTitle201 = new System.Windows.Forms.CheckBox();
      this.tp501 = new System.Windows.Forms.TabPage();
      this.Pan501 = new System.Windows.Forms.Panel();
      this.cbForwardEnabled501 = new System.Windows.Forms.CheckBox();
      this.cbBackEnabled501 = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbFinalStep501 = new System.Windows.Forms.CheckBox();
      this.cbTitle501 = new System.Windows.Forms.CheckBox();
      this.cbTitleForThisStepOnly501 = new System.Windows.Forms.CheckBox();
      this.cbHelpContext501 = new System.Windows.Forms.CheckBox();
      this.tp101.SuspendLayout();
      this.Pan101.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tp201.SuspendLayout();
      this.Pan201.SuspendLayout();
      this.tp501.SuspendLayout();
      this.Pan501.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tp101
      // 
      this.tp101.Controls.Add(this.Pan101);
      this.tp101.Location = new System.Drawing.Point(4, 22);
      this.tp101.Name = "tp101";
      this.tp101.Padding = new System.Windows.Forms.Padding(3);
      this.tp101.Size = new System.Drawing.Size(509, 330);
      this.tp101.TabIndex = 0;
      this.tp101.Text = "101";
      this.tp101.UseVisualStyleBackColor = true;
      // 
      // Pan101
      // 
      this.Pan101.Controls.Add(this.edTest101);
      this.Pan101.Controls.Add(this.label1);
      this.Pan101.Dock = System.Windows.Forms.DockStyle.Fill;
      this.Pan101.Location = new System.Drawing.Point(3, 3);
      this.Pan101.Name = "Pan101";
      this.Pan101.Size = new System.Drawing.Size(503, 324);
      this.Pan101.TabIndex = 0;
      // 
      // edTest101
      // 
      this.edTest101.Location = new System.Drawing.Point(126, 37);
      this.edTest101.Name = "edTest101";
      this.edTest101.Size = new System.Drawing.Size(356, 20);
      this.edTest101.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(5, 37);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Текстовое поле";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tp101);
      this.tabControl1.Controls.Add(this.tp201);
      this.tabControl1.Controls.Add(this.tp501);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(517, 356);
      this.tabControl1.TabIndex = 0;
      // 
      // tp201
      // 
      this.tp201.Controls.Add(this.Pan201);
      this.tp201.Location = new System.Drawing.Point(4, 22);
      this.tp201.Name = "tp201";
      this.tp201.Padding = new System.Windows.Forms.Padding(3);
      this.tp201.Size = new System.Drawing.Size(509, 330);
      this.tp201.TabIndex = 1;
      this.tp201.Text = "201";
      this.tp201.UseVisualStyleBackColor = true;
      // 
      // Pan201
      // 
      this.Pan201.Controls.Add(this.cbInfoIconSize201);
      this.Pan201.Controls.Add(this.label4);
      this.Pan201.Controls.Add(this.cbInfoIcon201);
      this.Pan201.Controls.Add(this.label3);
      this.Pan201.Controls.Add(this.cbInfoText201);
      this.Pan201.Controls.Add(this.label2);
      this.Pan201.Controls.Add(this.cbGroupTitle201);
      this.Pan201.Dock = System.Windows.Forms.DockStyle.Fill;
      this.Pan201.Location = new System.Drawing.Point(3, 3);
      this.Pan201.Name = "Pan201";
      this.Pan201.Size = new System.Drawing.Size(503, 324);
      this.Pan201.TabIndex = 0;
      // 
      // cbInfoIconSize201
      // 
      this.cbInfoIconSize201.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbInfoIconSize201.FormattingEnabled = true;
      this.cbInfoIconSize201.Items.AddRange(new object[] {
            "Small",
            "Large"});
      this.cbInfoIconSize201.Location = new System.Drawing.Point(144, 125);
      this.cbInfoIconSize201.Name = "cbInfoIconSize201";
      this.cbInfoIconSize201.Size = new System.Drawing.Size(334, 21);
      this.cbInfoIconSize201.TabIndex = 11;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(12, 125);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(114, 21);
      this.label4.TabIndex = 10;
      this.label4.Text = "InfoIcon";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbInfoIcon201
      // 
      this.cbInfoIcon201.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbInfoIcon201.FormattingEnabled = true;
      this.cbInfoIcon201.Items.AddRange(new object[] {
            "None",
            "Information",
            "Warning",
            "Error"});
      this.cbInfoIcon201.Location = new System.Drawing.Point(144, 87);
      this.cbInfoIcon201.Name = "cbInfoIcon201";
      this.cbInfoIcon201.Size = new System.Drawing.Size(334, 21);
      this.cbInfoIcon201.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 87);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(114, 21);
      this.label3.TabIndex = 8;
      this.label3.Text = "InfoIcon";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbInfoText201
      // 
      this.cbInfoText201.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbInfoText201.FormattingEnabled = true;
      this.cbInfoText201.Items.AddRange(new object[] {
            "Нет",
            "Короткий текст",
            "Длинный текст"});
      this.cbInfoText201.Location = new System.Drawing.Point(144, 50);
      this.cbInfoText201.Name = "cbInfoText201";
      this.cbInfoText201.Size = new System.Drawing.Size(334, 21);
      this.cbInfoText201.TabIndex = 7;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 50);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(114, 21);
      this.label2.TabIndex = 6;
      this.label2.Text = "InfoText";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbGroupTitle201
      // 
      this.cbGroupTitle201.AutoSize = true;
      this.cbGroupTitle201.Location = new System.Drawing.Point(15, 20);
      this.cbGroupTitle201.Name = "cbGroupTitle201";
      this.cbGroupTitle201.Size = new System.Drawing.Size(75, 17);
      this.cbGroupTitle201.TabIndex = 5;
      this.cbGroupTitle201.Text = "GroupTitle";
      this.cbGroupTitle201.UseVisualStyleBackColor = true;
      // 
      // tp501
      // 
      this.tp501.Controls.Add(this.Pan501);
      this.tp501.Location = new System.Drawing.Point(4, 22);
      this.tp501.Name = "tp501";
      this.tp501.Padding = new System.Windows.Forms.Padding(3);
      this.tp501.Size = new System.Drawing.Size(509, 330);
      this.tp501.TabIndex = 2;
      this.tp501.Text = "501";
      this.tp501.UseVisualStyleBackColor = true;
      // 
      // Pan501
      // 
      this.Pan501.Controls.Add(this.groupBox1);
      this.Pan501.Dock = System.Windows.Forms.DockStyle.Fill;
      this.Pan501.Location = new System.Drawing.Point(3, 3);
      this.Pan501.Name = "Pan501";
      this.Pan501.Size = new System.Drawing.Size(503, 324);
      this.Pan501.TabIndex = 0;
      // 
      // cbForwardEnabled501
      // 
      this.cbForwardEnabled501.AutoSize = true;
      this.cbForwardEnabled501.Location = new System.Drawing.Point(17, 56);
      this.cbForwardEnabled501.Name = "cbForwardEnabled501";
      this.cbForwardEnabled501.Size = new System.Drawing.Size(103, 17);
      this.cbForwardEnabled501.TabIndex = 0;
      this.cbForwardEnabled501.Text = "ForwardEnabled";
      this.cbForwardEnabled501.UseVisualStyleBackColor = true;
      // 
      // cbBackEnabled501
      // 
      this.cbBackEnabled501.AutoSize = true;
      this.cbBackEnabled501.Location = new System.Drawing.Point(17, 79);
      this.cbBackEnabled501.Name = "cbBackEnabled501";
      this.cbBackEnabled501.Size = new System.Drawing.Size(90, 17);
      this.cbBackEnabled501.TabIndex = 1;
      this.cbBackEnabled501.Text = "BackEnabled";
      this.cbBackEnabled501.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbHelpContext501);
      this.groupBox1.Controls.Add(this.cbTitleForThisStepOnly501);
      this.groupBox1.Controls.Add(this.cbTitle501);
      this.groupBox1.Controls.Add(this.cbFinalStep501);
      this.groupBox1.Controls.Add(this.cbForwardEnabled501);
      this.groupBox1.Controls.Add(this.cbBackEnabled501);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(503, 324);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Свойства для текущего WizardStep";
      // 
      // cbFinalStep501
      // 
      this.cbFinalStep501.AutoSize = true;
      this.cbFinalStep501.Location = new System.Drawing.Point(17, 33);
      this.cbFinalStep501.Name = "cbFinalStep501";
      this.cbFinalStep501.Size = new System.Drawing.Size(70, 17);
      this.cbFinalStep501.TabIndex = 2;
      this.cbFinalStep501.Text = "FinalStep";
      this.cbFinalStep501.UseVisualStyleBackColor = true;
      // 
      // cbTitle501
      // 
      this.cbTitle501.AutoSize = true;
      this.cbTitle501.Location = new System.Drawing.Point(17, 102);
      this.cbTitle501.Name = "cbTitle501";
      this.cbTitle501.Size = new System.Drawing.Size(46, 17);
      this.cbTitle501.TabIndex = 3;
      this.cbTitle501.Text = "Title";
      this.cbTitle501.UseVisualStyleBackColor = true;
      // 
      // cbTitleForThisStepOnly501
      // 
      this.cbTitleForThisStepOnly501.AutoSize = true;
      this.cbTitleForThisStepOnly501.Location = new System.Drawing.Point(17, 125);
      this.cbTitleForThisStepOnly501.Name = "cbTitleForThisStepOnly501";
      this.cbTitleForThisStepOnly501.Size = new System.Drawing.Size(124, 17);
      this.cbTitleForThisStepOnly501.TabIndex = 4;
      this.cbTitleForThisStepOnly501.Text = "TitleForThisStepOnly";
      this.cbTitleForThisStepOnly501.UseVisualStyleBackColor = true;
      // 
      // cbHelpContext501
      // 
      this.cbHelpContext501.AutoSize = true;
      this.cbHelpContext501.Location = new System.Drawing.Point(17, 148);
      this.cbHelpContext501.Name = "cbHelpContext501";
      this.cbHelpContext501.Size = new System.Drawing.Size(84, 17);
      this.cbHelpContext501.TabIndex = 5;
      this.cbHelpContext501.Text = "HelpContext";
      this.cbHelpContext501.UseVisualStyleBackColor = true;
      // 
      // TemplateForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(517, 356);
      this.Controls.Add(this.tabControl1);
      this.Name = "TemplateForm";
      this.Text = "TemplateForm";
      this.tp101.ResumeLayout(false);
      this.Pan101.ResumeLayout(false);
      this.Pan101.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tp201.ResumeLayout(false);
      this.Pan201.ResumeLayout(false);
      this.Pan201.PerformLayout();
      this.tp501.ResumeLayout(false);
      this.Pan501.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabPage tp101;
    private System.Windows.Forms.Panel Pan101;
    private System.Windows.Forms.TextBox edTest101;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tp201;
    private System.Windows.Forms.Panel Pan201;
    private System.Windows.Forms.ComboBox cbInfoIcon201;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbInfoText201;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox cbGroupTitle201;
    private System.Windows.Forms.ComboBox cbInfoIconSize201;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TabPage tp501;
    private System.Windows.Forms.Panel Pan501;
    private System.Windows.Forms.CheckBox cbBackEnabled501;
    private System.Windows.Forms.CheckBox cbForwardEnabled501;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox cbFinalStep501;
    private System.Windows.Forms.CheckBox cbTitleForThisStepOnly501;
    private System.Windows.Forms.CheckBox cbTitle501;
    private System.Windows.Forms.CheckBox cbHelpContext501;
  }
}