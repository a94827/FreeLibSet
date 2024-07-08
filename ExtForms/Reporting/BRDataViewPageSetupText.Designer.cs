﻿namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupText
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
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.cbExpHeaders = new System.Windows.Forms.CheckBox();
      this.cbRemoveDoubleSpaces = new System.Windows.Forms.CheckBox();
      this.cbSingleLineField = new System.Windows.Forms.CheckBox();
      this.cbQuote = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbFieldDelimiter = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbCodePage = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Location = new System.Drawing.Point(9, 10);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(436, 196);
      this.MainPanel.TabIndex = 0;
      // 
      // grpMain
      // 
      this.grpMain.Controls.Add(this.cbExpHeaders);
      this.grpMain.Controls.Add(this.cbRemoveDoubleSpaces);
      this.grpMain.Controls.Add(this.cbSingleLineField);
      this.grpMain.Controls.Add(this.cbQuote);
      this.grpMain.Controls.Add(this.label3);
      this.grpMain.Controls.Add(this.cbFieldDelimiter);
      this.grpMain.Controls.Add(this.label2);
      this.grpMain.Controls.Add(this.cbCodePage);
      this.grpMain.Controls.Add(this.label1);
      this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpMain.Location = new System.Drawing.Point(0, 0);
      this.grpMain.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grpMain.Name = "grpMain";
      this.grpMain.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grpMain.Size = new System.Drawing.Size(436, 196);
      this.grpMain.TabIndex = 0;
      this.grpMain.TabStop = false;
      this.grpMain.Text = "Параметры текстового файла";
      // 
      // cbExpHeaders
      // 
      this.cbExpHeaders.AutoSize = true;
      this.cbExpHeaders.Location = new System.Drawing.Point(15, 166);
      this.cbExpHeaders.Name = "cbExpHeaders";
      this.cbExpHeaders.Size = new System.Drawing.Size(130, 17);
      this.cbExpHeaders.TabIndex = 8;
      this.cbExpHeaders.Text = "Заголовки столбцов";
      this.cbExpHeaders.UseVisualStyleBackColor = true;
      // 
      // cbRemoveDoubleSpaces
      // 
      this.cbRemoveDoubleSpaces.AutoSize = true;
      this.cbRemoveDoubleSpaces.Location = new System.Drawing.Point(14, 144);
      this.cbRemoveDoubleSpaces.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbRemoveDoubleSpaces.Name = "cbRemoveDoubleSpaces";
      this.cbRemoveDoubleSpaces.Size = new System.Drawing.Size(225, 17);
      this.cbRemoveDoubleSpaces.TabIndex = 7;
      this.cbRemoveDoubleSpaces.Text = "Убирать двойные и концевые пробелы";
      this.cbRemoveDoubleSpaces.UseVisualStyleBackColor = true;
      // 
      // cbSingleLineField
      // 
      this.cbSingleLineField.AutoSize = true;
      this.cbSingleLineField.Location = new System.Drawing.Point(14, 123);
      this.cbSingleLineField.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbSingleLineField.Name = "cbSingleLineField";
      this.cbSingleLineField.Size = new System.Drawing.Size(223, 17);
      this.cbSingleLineField.TabIndex = 6;
      this.cbSingleLineField.Text = "Заменять переносы строк на пробелы";
      this.cbSingleLineField.UseVisualStyleBackColor = true;
      // 
      // cbQuote
      // 
      this.cbQuote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbQuote.FormattingEnabled = true;
      this.cbQuote.Items.AddRange(new object[] {
            "\"",
            "\'"});
      this.cbQuote.Location = new System.Drawing.Point(166, 89);
      this.cbQuote.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbQuote.Name = "cbQuote";
      this.cbQuote.Size = new System.Drawing.Size(254, 21);
      this.cbQuote.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 89);
      this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(149, 20);
      this.label3.TabIndex = 4;
      this.label3.Text = "Кавычки для строк";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbFieldDelimiter
      // 
      this.cbFieldDelimiter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFieldDelimiter.FormattingEnabled = true;
      this.cbFieldDelimiter.Items.AddRange(new object[] {
            ",",
            ";"});
      this.cbFieldDelimiter.Location = new System.Drawing.Point(166, 58);
      this.cbFieldDelimiter.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbFieldDelimiter.Name = "cbFieldDelimiter";
      this.cbFieldDelimiter.Size = new System.Drawing.Size(254, 21);
      this.cbFieldDelimiter.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 58);
      this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(149, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "Разделитель полей";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbCodePage
      // 
      this.cbCodePage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCodePage.FormattingEnabled = true;
      this.cbCodePage.Location = new System.Drawing.Point(166, 24);
      this.cbCodePage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbCodePage.Name = "cbCodePage";
      this.cbCodePage.Size = new System.Drawing.Size(254, 21);
      this.cbCodePage.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 24);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(149, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Кодировка";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // BRDataViewPageSetupText
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(458, 218);
      this.Controls.Add(this.MainPanel);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "BRDataViewPageSetupText";
      this.Text = "EFPDataViewExportTextForm";
      this.MainPanel.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.grpMain.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpMain;
    private System.Windows.Forms.ComboBox cbQuote;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbFieldDelimiter;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbCodePage;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox cbRemoveDoubleSpaces;
    private System.Windows.Forms.CheckBox cbSingleLineField;
    private System.Windows.Forms.CheckBox cbExpHeaders;
  }
}