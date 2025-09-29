namespace EFPCommandItemsDemo
{
  partial class HistDialogParamsForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbHistFileBrowserDialog = new System.Windows.Forms.RadioButton();
      this.rbHistFolderBrowserDialog = new System.Windows.Forms.RadioButton();
      this.panel2 = new System.Windows.Forms.Panel();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.cbShowSubFoldersButton = new System.Windows.Forms.CheckBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.edDescription = new System.Windows.Forms.TextBox();
      this.edDefaultPath = new System.Windows.Forms.TextBox();
      this.edSelectedPath = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.edCongfigSectionName = new System.Windows.Forms.TextBox();
      this.cbPathValidateMode = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbMode = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.cbSetSelectedPath = new System.Windows.Forms.CheckBox();
      this.edHistList = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edMaxHistLength = new FreeLibSet.Controls.Int32EditBox();
      this.edFilter = new System.Windows.Forms.TextBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 320);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(466, 40);
      this.panel1.TabIndex = 2;
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
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbHistFileBrowserDialog);
      this.groupBox1.Controls.Add(this.rbHistFolderBrowserDialog);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(466, 65);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Тип диалога";
      // 
      // rbHistFileBrowserDialog
      // 
      this.rbHistFileBrowserDialog.AutoSize = true;
      this.rbHistFileBrowserDialog.Location = new System.Drawing.Point(12, 42);
      this.rbHistFileBrowserDialog.Name = "rbHistFileBrowserDialog";
      this.rbHistFileBrowserDialog.Size = new System.Drawing.Size(127, 17);
      this.rbHistFileBrowserDialog.TabIndex = 1;
      this.rbHistFileBrowserDialog.TabStop = true;
      this.rbHistFileBrowserDialog.Text = "HistFileBrowserDialog";
      this.rbHistFileBrowserDialog.UseVisualStyleBackColor = true;
      // 
      // rbHistFolderBrowserDialog
      // 
      this.rbHistFolderBrowserDialog.AutoSize = true;
      this.rbHistFolderBrowserDialog.Location = new System.Drawing.Point(12, 19);
      this.rbHistFolderBrowserDialog.Name = "rbHistFolderBrowserDialog";
      this.rbHistFolderBrowserDialog.Size = new System.Drawing.Size(140, 17);
      this.rbHistFolderBrowserDialog.TabIndex = 0;
      this.rbHistFolderBrowserDialog.TabStop = true;
      this.rbHistFolderBrowserDialog.Text = "HistFolderBrowserDialog";
      this.rbHistFolderBrowserDialog.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.groupBox4);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 65);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(466, 255);
      this.panel2.TabIndex = 1;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.tableLayoutPanel1);
      this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox4.Location = new System.Drawing.Point(0, 0);
      this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox4.Size = new System.Drawing.Size(466, 255);
      this.groupBox4.TabIndex = 2;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Свойства";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.label8, 0, 8);
      this.tableLayoutPanel1.Controls.Add(this.cbShowSubFoldersButton, 1, 7);
      this.tableLayoutPanel1.Controls.Add(this.label5, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.edDescription, 2, 6);
      this.tableLayoutPanel1.Controls.Add(this.edDefaultPath, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.edSelectedPath, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.label6, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.edCongfigSectionName, 2, 5);
      this.tableLayoutPanel1.Controls.Add(this.cbPathValidateMode, 2, 4);
      this.tableLayoutPanel1.Controls.Add(this.label1, 1, 5);
      this.tableLayoutPanel1.Controls.Add(this.cbMode, 2, 3);
      this.tableLayoutPanel1.Controls.Add(this.label3, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label4, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.cbSetSelectedPath, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.edHistList, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.edMaxHistLength, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.edFilter, 2, 8);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 15);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 9;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(462, 238);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // cbShowSubFoldersButton
      // 
      this.cbShowSubFoldersButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbShowSubFoldersButton.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.cbShowSubFoldersButton, 2);
      this.cbShowSubFoldersButton.Location = new System.Drawing.Point(177, 186);
      this.cbShowSubFoldersButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbShowSubFoldersButton.Name = "cbShowSubFoldersButton";
      this.cbShowSubFoldersButton.Size = new System.Drawing.Size(283, 17);
      this.cbShowSubFoldersButton.TabIndex = 16;
      this.cbShowSubFoldersButton.Text = "ShowSubFoldersButton";
      this.cbShowSubFoldersButton.UseVisualStyleBackColor = true;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.Location = new System.Drawing.Point(178, 205);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(106, 21);
      this.label8.TabIndex = 17;
      this.label8.Text = "Filter";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.Location = new System.Drawing.Point(178, 158);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(106, 20);
      this.label5.TabIndex = 14;
      this.label5.Text = "Description";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDescription
      // 
      this.edDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edDescription.Location = new System.Drawing.Point(290, 161);
      this.edDescription.Name = "edDescription";
      this.edDescription.Size = new System.Drawing.Size(169, 20);
      this.edDescription.TabIndex = 15;
      // 
      // edDefaultPath
      // 
      this.edDefaultPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edDefaultPath.Location = new System.Drawing.Point(290, 55);
      this.edDefaultPath.Name = "edDefaultPath";
      this.edDefaultPath.Size = new System.Drawing.Size(169, 20);
      this.edDefaultPath.TabIndex = 7;
      // 
      // edSelectedPath
      // 
      this.edSelectedPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edSelectedPath.Location = new System.Drawing.Point(290, 29);
      this.edSelectedPath.Name = "edSelectedPath";
      this.edSelectedPath.Size = new System.Drawing.Size(169, 20);
      this.edSelectedPath.TabIndex = 5;
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.Location = new System.Drawing.Point(178, 52);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(106, 20);
      this.label6.TabIndex = 6;
      this.label6.Text = "DefaultPath";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edCongfigSectionName
      // 
      this.edCongfigSectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edCongfigSectionName.Location = new System.Drawing.Point(290, 135);
      this.edCongfigSectionName.Name = "edCongfigSectionName";
      this.edCongfigSectionName.Size = new System.Drawing.Size(169, 20);
      this.edCongfigSectionName.TabIndex = 13;
      // 
      // cbPathValidateMode
      // 
      this.cbPathValidateMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbPathValidateMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPathValidateMode.FormattingEnabled = true;
      this.cbPathValidateMode.Location = new System.Drawing.Point(290, 108);
      this.cbPathValidateMode.Name = "cbPathValidateMode";
      this.cbPathValidateMode.Size = new System.Drawing.Size(169, 21);
      this.cbPathValidateMode.TabIndex = 11;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(178, 132);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(106, 20);
      this.label1.TabIndex = 12;
      this.label1.Text = "ConfigSectionName";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbMode
      // 
      this.cbMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMode.FormattingEnabled = true;
      this.cbMode.Location = new System.Drawing.Point(290, 81);
      this.cbMode.Name = "cbMode";
      this.cbMode.Size = new System.Drawing.Size(169, 21);
      this.cbMode.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(178, 105);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(106, 21);
      this.label3.TabIndex = 10;
      this.label3.Text = "PathValidateMode";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(2, 13);
      this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(171, 13);
      this.label7.TabIndex = 0;
      this.label7.Text = "HistList";
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.Location = new System.Drawing.Point(178, 78);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(106, 21);
      this.label4.TabIndex = 8;
      this.label4.Text = "Mode";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbSetSelectedPath
      // 
      this.cbSetSelectedPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbSetSelectedPath.AutoSize = true;
      this.cbSetSelectedPath.Location = new System.Drawing.Point(177, 28);
      this.cbSetSelectedPath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbSetSelectedPath.Name = "cbSetSelectedPath";
      this.cbSetSelectedPath.Size = new System.Drawing.Size(108, 17);
      this.cbSetSelectedPath.TabIndex = 4;
      this.cbSetSelectedPath.Text = "SelectedPath";
      this.cbSetSelectedPath.UseVisualStyleBackColor = true;
      // 
      // edHistList
      // 
      this.edHistList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edHistList.Location = new System.Drawing.Point(3, 29);
      this.edHistList.Multiline = true;
      this.edHistList.Name = "edHistList";
      this.tableLayoutPanel1.SetRowSpan(this.edHistList, 8);
      this.edHistList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.edHistList.Size = new System.Drawing.Size(169, 206);
      this.edHistList.TabIndex = 1;
      this.edHistList.WordWrap = false;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(178, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(106, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "MaxHistLength";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edMaxHistLength
      // 
      this.edMaxHistLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edMaxHistLength.Increment = 1;
      this.edMaxHistLength.Location = new System.Drawing.Point(290, 3);
      this.edMaxHistLength.Maximum = 1000;
      this.edMaxHistLength.Minimum = 1;
      this.edMaxHistLength.Name = "edMaxHistLength";
      this.edMaxHistLength.NValue = 1;
      this.edMaxHistLength.Size = new System.Drawing.Size(169, 20);
      this.edMaxHistLength.TabIndex = 3;
      // 
      // edFilter
      // 
      this.edFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edFilter.Location = new System.Drawing.Point(290, 208);
      this.edFilter.Name = "edFilter";
      this.edFilter.Size = new System.Drawing.Size(169, 20);
      this.edFilter.TabIndex = 18;
      // 
      // HistDialogParamsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(466, 360);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.groupBox1);
      this.Name = "HistDialogParamsForm";
      this.Text = "HistFolderBrowserDialog и HistFileBrowserDialog";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.RadioButton rbHistFileBrowserDialog;
    private System.Windows.Forms.RadioButton rbHistFolderBrowserDialog;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.TextBox edCongfigSectionName;
    private System.Windows.Forms.Label label1;
    private FreeLibSet.Controls.Int32EditBox edMaxHistLength;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbPathValidateMode;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbMode;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.CheckBox cbShowSubFoldersButton;
    private System.Windows.Forms.TextBox edDescription;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox edDefaultPath;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox edSelectedPath;
    private System.Windows.Forms.CheckBox cbSetSelectedPath;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox edHistList;
    private System.Windows.Forms.TextBox edFilter;
  }
}