namespace FIASDemo
{
  partial class DownloadFilesForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.edDir = new System.Windows.Forms.TextBox();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.edActualDate = new AgeyevAV.ExtForms.DateBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbFormat = new System.Windows.Forms.ComboBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 147);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 40);
      this.panel1.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbFormat);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.edActualDate);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.btnBrowse);
      this.groupBox1.Controls.Add(this.edDir);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(632, 147);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 22);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(127, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Куда скачать";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDir
      // 
      this.edDir.Location = new System.Drawing.Point(145, 22);
      this.edDir.Name = "edDir";
      this.edDir.Size = new System.Drawing.Size(374, 20);
      this.edDir.TabIndex = 1;
      // 
      // btnBrowse
      // 
      this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowse.Location = new System.Drawing.Point(532, 19);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(88, 24);
      this.btnBrowse.TabIndex = 2;
      this.btnBrowse.Text = "Обзор";
      this.btnBrowse.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 62);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(337, 20);
      this.label2.TabIndex = 3;
      this.label2.Text = "Скачать обновления, новее чем (текущая дата актульности)";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edActualDate
      // 
      this.edActualDate.Location = new System.Drawing.Point(399, 62);
      this.edActualDate.Name = "edActualDate";
      this.edActualDate.Size = new System.Drawing.Size(120, 20);
      this.edActualDate.TabIndex = 4;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 103);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(127, 20);
      this.label3.TabIndex = 5;
      this.label3.Text = "Формат обновлений";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbFormat
      // 
      this.cbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFormat.FormattingEnabled = true;
      this.cbFormat.Location = new System.Drawing.Point(145, 102);
      this.cbFormat.Name = "cbFormat";
      this.cbFormat.Size = new System.Drawing.Size(199, 21);
      this.cbFormat.TabIndex = 6;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // DownloadFilesForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(632, 187);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Name = "DownloadFilesForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Скачивание файлов обновлений";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private AgeyevAV.ExtForms.DateBox edActualDate;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.TextBox edDir;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbFormat;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
  }
}