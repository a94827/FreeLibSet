namespace FreeLibSet.Forms
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileCopierForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.grpCheck = new System.Windows.Forms.GroupBox();
      this.pbCheck = new System.Windows.Forms.ProgressBar();
      this.grpTotal = new System.Windows.Forms.GroupBox();
      this.pbTotal = new System.Windows.Forms.ProgressBar();
      this.txtTotalBytes = new System.Windows.Forms.Label();
      this.lblTotalBytes = new System.Windows.Forms.Label();
      this.txtTotalFiles = new System.Windows.Forms.Label();
      this.lblTotalFiles = new System.Windows.Forms.Label();
      this.txtCopiedBytes = new System.Windows.Forms.Label();
      this.lblCopiedBytes = new System.Windows.Forms.Label();
      this.txtCopiedFiles = new System.Windows.Forms.Label();
      this.lblCopiedFiles = new System.Windows.Forms.Label();
      this.grpCurrentFile = new System.Windows.Forms.GroupBox();
      this.pbFile = new System.Windows.Forms.ProgressBar();
      this.txtFileSize = new System.Windows.Forms.Label();
      this.lblFileSize = new System.Windows.Forms.Label();
      this.txtFileName = new System.Windows.Forms.Label();
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
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Name = "panel1";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.tableLayoutPanel1);
      this.MainPanel.Name = "MainPanel";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 1);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.grpCheck);
      this.panel2.Controls.Add(this.grpTotal);
      this.panel2.Controls.Add(this.grpCurrentFile);
      this.panel2.Name = "panel2";
      // 
      // grpCheck
      // 
      resources.ApplyResources(this.grpCheck, "grpCheck");
      this.grpCheck.Controls.Add(this.pbCheck);
      this.grpCheck.Name = "grpCheck";
      this.grpCheck.TabStop = false;
      // 
      // pbCheck
      // 
      resources.ApplyResources(this.pbCheck, "pbCheck");
      this.pbCheck.Name = "pbCheck";
      // 
      // grpTotal
      // 
      resources.ApplyResources(this.grpTotal, "grpTotal");
      this.grpTotal.Controls.Add(this.pbTotal);
      this.grpTotal.Controls.Add(this.txtTotalBytes);
      this.grpTotal.Controls.Add(this.lblTotalBytes);
      this.grpTotal.Controls.Add(this.txtTotalFiles);
      this.grpTotal.Controls.Add(this.lblTotalFiles);
      this.grpTotal.Controls.Add(this.txtCopiedBytes);
      this.grpTotal.Controls.Add(this.lblCopiedBytes);
      this.grpTotal.Controls.Add(this.txtCopiedFiles);
      this.grpTotal.Controls.Add(this.lblCopiedFiles);
      this.grpTotal.Name = "grpTotal";
      this.grpTotal.TabStop = false;
      // 
      // pbTotal
      // 
      resources.ApplyResources(this.pbTotal, "pbTotal");
      this.pbTotal.Name = "pbTotal";
      // 
      // txtTotalBytes
      // 
      resources.ApplyResources(this.txtTotalBytes, "txtTotalBytes");
      this.txtTotalBytes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtTotalBytes.Name = "txtTotalBytes";
      // 
      // lblTotalBytes
      // 
      resources.ApplyResources(this.lblTotalBytes, "lblTotalBytes");
      this.lblTotalBytes.Name = "lblTotalBytes";
      // 
      // txtTotalFiles
      // 
      resources.ApplyResources(this.txtTotalFiles, "txtTotalFiles");
      this.txtTotalFiles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtTotalFiles.Name = "txtTotalFiles";
      // 
      // lblTotalFiles
      // 
      resources.ApplyResources(this.lblTotalFiles, "lblTotalFiles");
      this.lblTotalFiles.Name = "lblTotalFiles";
      // 
      // txtCopiedBytes
      // 
      resources.ApplyResources(this.txtCopiedBytes, "txtCopiedBytes");
      this.txtCopiedBytes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtCopiedBytes.Name = "txtCopiedBytes";
      // 
      // lblCopiedBytes
      // 
      resources.ApplyResources(this.lblCopiedBytes, "lblCopiedBytes");
      this.lblCopiedBytes.Name = "lblCopiedBytes";
      // 
      // txtCopiedFiles
      // 
      resources.ApplyResources(this.txtCopiedFiles, "txtCopiedFiles");
      this.txtCopiedFiles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtCopiedFiles.Name = "txtCopiedFiles";
      // 
      // lblCopiedFiles
      // 
      resources.ApplyResources(this.lblCopiedFiles, "lblCopiedFiles");
      this.lblCopiedFiles.Name = "lblCopiedFiles";
      // 
      // grpCurrentFile
      // 
      resources.ApplyResources(this.grpCurrentFile, "grpCurrentFile");
      this.grpCurrentFile.Controls.Add(this.pbFile);
      this.grpCurrentFile.Controls.Add(this.txtFileSize);
      this.grpCurrentFile.Controls.Add(this.lblFileSize);
      this.grpCurrentFile.Controls.Add(this.txtFileName);
      this.grpCurrentFile.Name = "grpCurrentFile";
      this.grpCurrentFile.TabStop = false;
      // 
      // pbFile
      // 
      resources.ApplyResources(this.pbFile, "pbFile");
      this.pbFile.Name = "pbFile";
      // 
      // txtFileSize
      // 
      resources.ApplyResources(this.txtFileSize, "txtFileSize");
      this.txtFileSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtFileSize.Name = "txtFileSize";
      // 
      // lblFileSize
      // 
      resources.ApplyResources(this.lblFileSize, "lblFileSize");
      this.lblFileSize.Name = "lblFileSize";
      // 
      // txtFileName
      // 
      resources.ApplyResources(this.txtFileName, "txtFileName");
      this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.txtFileName.Name = "txtFileName";
      // 
      // FileCopierForm
      // 
      this.AcceptButton = this.btnCancel;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FileCopierForm";
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
    private System.Windows.Forms.Label lblTotalBytes;
    private System.Windows.Forms.Label lblTotalFiles;
    private System.Windows.Forms.Label lblCopiedBytes;
    private System.Windows.Forms.Label lblCopiedFiles;
    private System.Windows.Forms.Label lblFileSize;
    public System.Windows.Forms.GroupBox grpTotal;
    public System.Windows.Forms.ProgressBar pbTotal;
    public System.Windows.Forms.Label txtTotalBytes;
    public System.Windows.Forms.Label txtTotalFiles;
    public System.Windows.Forms.Label txtCopiedBytes;
    public System.Windows.Forms.Label txtCopiedFiles;
    public System.Windows.Forms.GroupBox grpCurrentFile;
    public System.Windows.Forms.ProgressBar pbFile;
    public System.Windows.Forms.Label txtFileSize;
    public System.Windows.Forms.Label txtFileName;
    public System.Windows.Forms.GroupBox grpCheck;
    public System.Windows.Forms.ProgressBar pbCheck;
    public System.Windows.Forms.Panel MainPanel;
  }
}
