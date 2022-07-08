namespace FileEnumeratorDemo
{
  partial class ParamForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbClass = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edRootDirectory = new System.Windows.Forms.TextBox();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.cbEnumerateKind = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbEnumerateMode = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.grpFiles = new System.Windows.Forms.GroupBox();
      this.edFileSearchPattern = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this.cbFileSort = new System.Windows.Forms.ComboBox();
      this.label6 = new System.Windows.Forms.Label();
      this.cbReverseFiles = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbReverseDirectories = new System.Windows.Forms.CheckBox();
      this.cbDirectorySort = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.edDirectorySearchPattern = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.grpFiles.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbEnumerateMode);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbEnumerateKind);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.btnBrowse);
      this.groupBox1.Controls.Add(this.edRootDirectory);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbClass);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(711, 155);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 32);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(134, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Класс перечислителя";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbClass
      // 
      this.cbClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbClass.FormattingEnabled = true;
      this.cbClass.Location = new System.Drawing.Point(173, 35);
      this.cbClass.Name = "cbClass";
      this.cbClass.Size = new System.Drawing.Size(526, 21);
      this.cbClass.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(134, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "RootDirectory";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edRootDirectory
      // 
      this.edRootDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edRootDirectory.Location = new System.Drawing.Point(173, 67);
      this.edRootDirectory.Name = "edRootDirectory";
      this.edRootDirectory.Size = new System.Drawing.Size(432, 20);
      this.edRootDirectory.TabIndex = 3;
      // 
      // btnBrowse
      // 
      this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowse.Location = new System.Drawing.Point(611, 64);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(88, 24);
      this.btnBrowse.TabIndex = 4;
      this.btnBrowse.Text = "Обзор";
      this.btnBrowse.UseVisualStyleBackColor = true;
      // 
      // cbEnumerateKind
      // 
      this.cbEnumerateKind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbEnumerateKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEnumerateKind.FormattingEnabled = true;
      this.cbEnumerateKind.Location = new System.Drawing.Point(173, 94);
      this.cbEnumerateKind.Name = "cbEnumerateKind";
      this.cbEnumerateKind.Size = new System.Drawing.Size(526, 21);
      this.cbEnumerateKind.TabIndex = 6;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 91);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(134, 23);
      this.label3.TabIndex = 5;
      this.label3.Text = "EnumerateKind";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbEnumerateMode
      // 
      this.cbEnumerateMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbEnumerateMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEnumerateMode.FormattingEnabled = true;
      this.cbEnumerateMode.Location = new System.Drawing.Point(173, 121);
      this.cbEnumerateMode.Name = "cbEnumerateMode";
      this.cbEnumerateMode.Size = new System.Drawing.Size(526, 21);
      this.cbEnumerateMode.TabIndex = 8;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 118);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(134, 23);
      this.label4.TabIndex = 7;
      this.label4.Text = "EnumerateMode";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpFiles
      // 
      this.grpFiles.Controls.Add(this.cbReverseFiles);
      this.grpFiles.Controls.Add(this.cbFileSort);
      this.grpFiles.Controls.Add(this.label6);
      this.grpFiles.Controls.Add(this.edFileSearchPattern);
      this.grpFiles.Controls.Add(this.label5);
      this.grpFiles.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpFiles.Location = new System.Drawing.Point(0, 155);
      this.grpFiles.Name = "grpFiles";
      this.grpFiles.Size = new System.Drawing.Size(711, 78);
      this.grpFiles.TabIndex = 1;
      this.grpFiles.TabStop = false;
      this.grpFiles.Text = "Файлы";
      // 
      // edFileSearchPattern
      // 
      this.edFileSearchPattern.Location = new System.Drawing.Point(173, 19);
      this.edFileSearchPattern.Name = "edFileSearchPattern";
      this.edFileSearchPattern.Size = new System.Drawing.Size(221, 20);
      this.edFileSearchPattern.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(16, 16);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(134, 23);
      this.label5.TabIndex = 0;
      this.label5.Text = "FileSearchPattern";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbFileSort
      // 
      this.cbFileSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFileSort.FormattingEnabled = true;
      this.cbFileSort.Location = new System.Drawing.Point(173, 45);
      this.cbFileSort.Name = "cbFileSort";
      this.cbFileSort.Size = new System.Drawing.Size(221, 21);
      this.cbFileSort.TabIndex = 3;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(16, 42);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(134, 23);
      this.label6.TabIndex = 2;
      this.label6.Text = "FileSort";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbReverseFiles
      // 
      this.cbReverseFiles.AutoSize = true;
      this.cbReverseFiles.Location = new System.Drawing.Point(410, 46);
      this.cbReverseFiles.Name = "cbReverseFiles";
      this.cbReverseFiles.Size = new System.Drawing.Size(87, 17);
      this.cbReverseFiles.TabIndex = 4;
      this.cbReverseFiles.Text = "ReverseFiles";
      this.cbReverseFiles.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbReverseDirectories);
      this.groupBox2.Controls.Add(this.cbDirectorySort);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.edDirectorySearchPattern);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 233);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(711, 78);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Каталоги";
      // 
      // cbReverseDirectories
      // 
      this.cbReverseDirectories.AutoSize = true;
      this.cbReverseDirectories.Location = new System.Drawing.Point(410, 46);
      this.cbReverseDirectories.Name = "cbReverseDirectories";
      this.cbReverseDirectories.Size = new System.Drawing.Size(116, 17);
      this.cbReverseDirectories.TabIndex = 4;
      this.cbReverseDirectories.Text = "ReverseDirectories";
      this.cbReverseDirectories.UseVisualStyleBackColor = true;
      // 
      // cbDirectorySort
      // 
      this.cbDirectorySort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDirectorySort.FormattingEnabled = true;
      this.cbDirectorySort.Location = new System.Drawing.Point(173, 45);
      this.cbDirectorySort.Name = "cbDirectorySort";
      this.cbDirectorySort.Size = new System.Drawing.Size(221, 21);
      this.cbDirectorySort.TabIndex = 3;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(16, 42);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(134, 23);
      this.label7.TabIndex = 2;
      this.label7.Text = "DirectorySort";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDirectorySearchPattern
      // 
      this.edDirectorySearchPattern.Location = new System.Drawing.Point(173, 19);
      this.edDirectorySearchPattern.Name = "edDirectorySearchPattern";
      this.edDirectorySearchPattern.Size = new System.Drawing.Size(221, 20);
      this.edDirectorySearchPattern.TabIndex = 1;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 16);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(134, 23);
      this.label8.TabIndex = 0;
      this.label8.Text = "DirectorySearchPattern";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(12, 317);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // ParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(711, 354);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.grpFiles);
      this.Controls.Add(this.groupBox1);
      this.Name = "ParamForm";
      this.Text = "Перебор каталогов и файлов";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.grpFiles.ResumeLayout(false);
      this.grpFiles.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbClass;
    private System.Windows.Forms.ComboBox cbEnumerateKind;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.TextBox edRootDirectory;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbEnumerateMode;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.GroupBox grpFiles;
    private System.Windows.Forms.TextBox edFileSearchPattern;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.ComboBox cbFileSort;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.CheckBox cbReverseFiles;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox cbReverseDirectories;
    private System.Windows.Forms.ComboBox cbDirectorySort;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox edDirectorySearchPattern;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Button btnOk;
  }
}

