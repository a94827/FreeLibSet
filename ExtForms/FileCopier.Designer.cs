namespace AgeyevAV.ExtForms
{
  partial class FileCopierForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.grpCheck = new System.Windows.Forms.GroupBox();
      this.pbCheck = new System.Windows.Forms.ProgressBar();
      this.grpTotal = new System.Windows.Forms.GroupBox();
      this.pbTotal = new System.Windows.Forms.ProgressBar();
      this.lblTotalBytes = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.lblTotalFiles = new System.Windows.Forms.Label();
      this.label11 = new System.Windows.Forms.Label();
      this.lblCopiedBytes = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.lblCopiedFiles = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.grpCurrentFile = new System.Windows.Forms.GroupBox();
      this.pbFile = new System.Windows.Forms.ProgressBar();
      this.lblFileSize = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.lblFileName = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.grpCheck.SuspendLayout();
      this.grpTotal.SuspendLayout();
      this.grpCurrentFile.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 248);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(407, 40);
      this.panel1.TabIndex = 0;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(159, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.tableLayoutPanel1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(407, 248);
      this.MainPanel.TabIndex = 1;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(407, 248);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.panel2.Controls.Add(this.grpCheck);
      this.panel2.Controls.Add(this.grpTotal);
      this.panel2.Controls.Add(this.grpCurrentFile);
      this.panel2.Location = new System.Drawing.Point(6, 6);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(394, 235);
      this.panel2.TabIndex = 0;
      // 
      // grpCheck
      // 
      this.grpCheck.Controls.Add(this.pbCheck);
      this.grpCheck.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpCheck.Location = new System.Drawing.Point(0, 182);
      this.grpCheck.Name = "grpCheck";
      this.grpCheck.Size = new System.Drawing.Size(394, 50);
      this.grpCheck.TabIndex = 4;
      this.grpCheck.TabStop = false;
      this.grpCheck.Text = "Проверка";
      this.grpCheck.Visible = false;
      // 
      // pbCheck
      // 
      this.pbCheck.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pbCheck.Location = new System.Drawing.Point(6, 19);
      this.pbCheck.Name = "pbCheck";
      this.pbCheck.Size = new System.Drawing.Size(385, 23);
      this.pbCheck.TabIndex = 11;
      // 
      // grpTotal
      // 
      this.grpTotal.Controls.Add(this.pbTotal);
      this.grpTotal.Controls.Add(this.lblTotalBytes);
      this.grpTotal.Controls.Add(this.label9);
      this.grpTotal.Controls.Add(this.lblTotalFiles);
      this.grpTotal.Controls.Add(this.label11);
      this.grpTotal.Controls.Add(this.lblCopiedBytes);
      this.grpTotal.Controls.Add(this.label7);
      this.grpTotal.Controls.Add(this.lblCopiedFiles);
      this.grpTotal.Controls.Add(this.label5);
      this.grpTotal.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpTotal.Location = new System.Drawing.Point(0, 93);
      this.grpTotal.Name = "grpTotal";
      this.grpTotal.Size = new System.Drawing.Size(394, 89);
      this.grpTotal.TabIndex = 3;
      this.grpTotal.TabStop = false;
      this.grpTotal.Text = "Всего";
      this.grpTotal.Visible = false;
      // 
      // pbTotal
      // 
      this.pbTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pbTotal.Location = new System.Drawing.Point(6, 59);
      this.pbTotal.Name = "pbTotal";
      this.pbTotal.Size = new System.Drawing.Size(382, 23);
      this.pbTotal.TabIndex = 10;
      // 
      // lblTotalBytes
      // 
      this.lblTotalBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblTotalBytes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblTotalBytes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblTotalBytes.Location = new System.Drawing.Point(225, 36);
      this.lblTotalBytes.Name = "lblTotalBytes";
      this.lblTotalBytes.Size = new System.Drawing.Size(163, 20);
      this.lblTotalBytes.TabIndex = 9;
      this.lblTotalBytes.Text = "???";
      this.lblTotalBytes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(176, 36);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(53, 20);
      this.label9.TabIndex = 8;
      this.label9.Text = "Размер";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTotalFiles
      // 
      this.lblTotalFiles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblTotalFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblTotalFiles.Location = new System.Drawing.Point(101, 36);
      this.lblTotalFiles.Name = "lblTotalFiles";
      this.lblTotalFiles.Size = new System.Drawing.Size(69, 20);
      this.lblTotalFiles.TabIndex = 7;
      this.lblTotalFiles.Text = "???";
      this.lblTotalFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(9, 36);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(86, 20);
      this.label11.TabIndex = 6;
      this.label11.Text = "из";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblCopiedBytes
      // 
      this.lblCopiedBytes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCopiedBytes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCopiedBytes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblCopiedBytes.Location = new System.Drawing.Point(225, 16);
      this.lblCopiedBytes.Name = "lblCopiedBytes";
      this.lblCopiedBytes.Size = new System.Drawing.Size(163, 20);
      this.lblCopiedBytes.TabIndex = 5;
      this.lblCopiedBytes.Text = "???";
      this.lblCopiedBytes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(176, 16);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(53, 20);
      this.label7.TabIndex = 4;
      this.label7.Text = "Размер";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblCopiedFiles
      // 
      this.lblCopiedFiles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCopiedFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblCopiedFiles.Location = new System.Drawing.Point(101, 16);
      this.lblCopiedFiles.Name = "lblCopiedFiles";
      this.lblCopiedFiles.Size = new System.Drawing.Size(69, 20);
      this.lblCopiedFiles.TabIndex = 3;
      this.lblCopiedFiles.Text = "???";
      this.lblCopiedFiles.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(9, 16);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(86, 20);
      this.label5.TabIndex = 2;
      this.label5.Text = "Скопировано";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpCurrentFile
      // 
      this.grpCurrentFile.Controls.Add(this.pbFile);
      this.grpCurrentFile.Controls.Add(this.lblFileSize);
      this.grpCurrentFile.Controls.Add(this.label3);
      this.grpCurrentFile.Controls.Add(this.lblFileName);
      this.grpCurrentFile.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpCurrentFile.Location = new System.Drawing.Point(0, 0);
      this.grpCurrentFile.Name = "grpCurrentFile";
      this.grpCurrentFile.Size = new System.Drawing.Size(394, 93);
      this.grpCurrentFile.TabIndex = 2;
      this.grpCurrentFile.TabStop = false;
      this.grpCurrentFile.Text = "Текущий файл";
      this.grpCurrentFile.Visible = false;
      // 
      // pbFile
      // 
      this.pbFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pbFile.Location = new System.Drawing.Point(6, 64);
      this.pbFile.Name = "pbFile";
      this.pbFile.Size = new System.Drawing.Size(382, 23);
      this.pbFile.TabIndex = 4;
      // 
      // lblFileSize
      // 
      this.lblFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFileSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblFileSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblFileSize.Location = new System.Drawing.Point(225, 41);
      this.lblFileSize.Name = "lblFileSize";
      this.lblFileSize.Size = new System.Drawing.Size(163, 20);
      this.lblFileSize.TabIndex = 3;
      this.lblFileSize.Text = "???";
      this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(176, 41);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(53, 20);
      this.label3.TabIndex = 2;
      this.label3.Text = "Размер";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblFileName
      // 
      this.lblFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblFileName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblFileName.Location = new System.Drawing.Point(6, 16);
      this.lblFileName.Name = "lblFileName";
      this.lblFileName.Size = new System.Drawing.Size(382, 20);
      this.lblFileName.TabIndex = 1;
      this.lblFileName.Text = "???";
      this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // FileCopierForm
      // 
      this.AcceptButton = this.btnCancel;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(407, 288);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FileCopierForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FileCopierForm_FormClosing);
      this.panel1.ResumeLayout(false);
      this.MainPanel.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.grpCheck.ResumeLayout(false);
      this.grpTotal.ResumeLayout(false);
      this.grpCurrentFile.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label3;
    public System.Windows.Forms.GroupBox grpTotal;
    public System.Windows.Forms.ProgressBar pbTotal;
    public System.Windows.Forms.Label lblTotalBytes;
    public System.Windows.Forms.Label lblTotalFiles;
    public System.Windows.Forms.Label lblCopiedBytes;
    public System.Windows.Forms.Label lblCopiedFiles;
    public System.Windows.Forms.GroupBox grpCurrentFile;
    public System.Windows.Forms.ProgressBar pbFile;
    public System.Windows.Forms.Label lblFileSize;
    public System.Windows.Forms.Label lblFileName;
    public System.Windows.Forms.GroupBox grpCheck;
    public System.Windows.Forms.ProgressBar pbCheck;
    public System.Windows.Forms.Panel MainPanel;
  }
}