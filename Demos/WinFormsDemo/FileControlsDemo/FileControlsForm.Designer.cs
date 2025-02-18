namespace WinFormsDemo.FileControlsDemo
{
  partial class FileControlsForm
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
      this.MainPanel = new System.Windows.Forms.Panel();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.cbPathValidateMode = new System.Windows.Forms.ComboBox();
      this.cbCanBeEmptyMode = new System.Windows.Forms.ComboBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.txt6 = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.br6 = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.br5 = new System.Windows.Forms.Button();
      this.label6 = new System.Windows.Forms.Label();
      this.txt4 = new System.Windows.Forms.TextBox();
      this.br4 = new System.Windows.Forms.Button();
      this.txt5 = new System.Windows.Forms.ComboBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.txt3 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.br3 = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.br2 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.txt1 = new System.Windows.Forms.TextBox();
      this.br1 = new System.Windows.Forms.Button();
      this.txt2 = new System.Windows.Forms.ComboBox();
      this.panel1.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tableLayoutPanel3.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 340);
      this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(709, 49);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(133, 10);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 30);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(11, 10);
      this.btnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(117, 30);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "O&K";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.groupBox3);
      this.MainPanel.Controls.Add(this.groupBox2);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(709, 340);
      this.MainPanel.TabIndex = 0;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.tableLayoutPanel3);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Location = new System.Drawing.Point(0, 242);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox3.Size = new System.Drawing.Size(709, 100);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Controllimg properties";
      // 
      // tableLayoutPanel3
      // 
      this.tableLayoutPanel3.AutoSize = true;
      this.tableLayoutPanel3.ColumnCount = 2;
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel3.Controls.Add(this.label8, 0, 1);
      this.tableLayoutPanel3.Controls.Add(this.label7, 0, 0);
      this.tableLayoutPanel3.Controls.Add(this.cbPathValidateMode, 1, 1);
      this.tableLayoutPanel3.Controls.Add(this.cbCanBeEmptyMode, 1, 0);
      this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 17);
      this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel3.Name = "tableLayoutPanel3";
      this.tableLayoutPanel3.RowCount = 3;
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel3.Size = new System.Drawing.Size(703, 56);
      this.tableLayoutPanel3.TabIndex = 1;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(3, 28);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(143, 28);
      this.label8.TabIndex = 3;
      this.label8.Text = "PathValidateMode";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(3, 0);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(143, 28);
      this.label7.TabIndex = 0;
      this.label7.Text = "CanBeEmptyMode";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbPathValidateMode
      // 
      this.cbPathValidateMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbPathValidateMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPathValidateMode.FormattingEnabled = true;
      this.cbPathValidateMode.Location = new System.Drawing.Point(152, 30);
      this.cbPathValidateMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbPathValidateMode.Name = "cbPathValidateMode";
      this.cbPathValidateMode.Size = new System.Drawing.Size(548, 24);
      this.cbPathValidateMode.TabIndex = 4;
      // 
      // cbCanBeEmptyMode
      // 
      this.cbCanBeEmptyMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCanBeEmptyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCanBeEmptyMode.FormattingEnabled = true;
      this.cbCanBeEmptyMode.Location = new System.Drawing.Point(152, 2);
      this.cbCanBeEmptyMode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbCanBeEmptyMode.Name = "cbCanBeEmptyMode";
      this.cbCanBeEmptyMode.Size = new System.Drawing.Size(548, 24);
      this.cbCanBeEmptyMode.TabIndex = 5;
      // 
      // groupBox2
      // 
      this.groupBox2.AutoSize = true;
      this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.groupBox2.Controls.Add(this.tableLayoutPanel2);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 121);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox2.Size = new System.Drawing.Size(709, 121);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "EFPFileDialogButton";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.AutoSize = true;
      this.tableLayoutPanel2.ColumnCount = 3;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel2.Controls.Add(this.txt6, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.label4, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.br6, 2, 2);
      this.tableLayoutPanel2.Controls.Add(this.label5, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.br5, 2, 1);
      this.tableLayoutPanel2.Controls.Add(this.label6, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.txt4, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.br4, 2, 0);
      this.tableLayoutPanel2.Controls.Add(this.txt5, 1, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 17);
      this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 3;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.Size = new System.Drawing.Size(703, 102);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // txt6
      // 
      this.txt6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt6.Location = new System.Drawing.Point(152, 70);
      this.txt6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt6.Name = "txt6";
      this.txt6.ReadOnly = true;
      this.txt6.Size = new System.Drawing.Size(425, 22);
      this.txt6.TabIndex = 9;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(3, 68);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(143, 34);
      this.label4.TabIndex = 6;
      this.label4.Text = "EFPTextBox RO";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // br6
      // 
      this.br6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br6.Location = new System.Drawing.Point(583, 70);
      this.br6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br6.Name = "br6";
      this.br6.Size = new System.Drawing.Size(117, 30);
      this.br6.TabIndex = 8;
      this.br6.Text = "Browse";
      this.br6.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(3, 34);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(143, 34);
      this.label5.TabIndex = 3;
      this.label5.Text = "EFPHistComboBox";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // br5
      // 
      this.br5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br5.Location = new System.Drawing.Point(583, 36);
      this.br5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br5.Name = "br5";
      this.br5.Size = new System.Drawing.Size(117, 30);
      this.br5.TabIndex = 5;
      this.br5.Text = "Browse";
      this.br5.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(3, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(143, 34);
      this.label6.TabIndex = 0;
      this.label6.Text = "EFPTextBox";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txt4
      // 
      this.txt4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt4.Location = new System.Drawing.Point(152, 2);
      this.txt4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt4.Name = "txt4";
      this.txt4.Size = new System.Drawing.Size(425, 22);
      this.txt4.TabIndex = 1;
      // 
      // br4
      // 
      this.br4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br4.Location = new System.Drawing.Point(583, 2);
      this.br4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br4.Name = "br4";
      this.br4.Size = new System.Drawing.Size(117, 30);
      this.br4.TabIndex = 2;
      this.br4.Text = "Browse";
      this.br4.UseVisualStyleBackColor = true;
      // 
      // txt5
      // 
      this.txt5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt5.FormattingEnabled = true;
      this.txt5.Location = new System.Drawing.Point(152, 36);
      this.txt5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt5.Name = "txt5";
      this.txt5.Size = new System.Drawing.Size(425, 24);
      this.txt5.TabIndex = 4;
      // 
      // groupBox1
      // 
      this.groupBox1.AutoSize = true;
      this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.groupBox1.Controls.Add(this.tableLayoutPanel1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox1.Size = new System.Drawing.Size(709, 121);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "EFPFolderBrowserButton";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 149F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.txt3, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.br3, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.br2, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.txt1, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.br1, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.txt2, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 17);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(703, 102);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // txt3
      // 
      this.txt3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt3.Location = new System.Drawing.Point(152, 70);
      this.txt3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt3.Name = "txt3";
      this.txt3.ReadOnly = true;
      this.txt3.Size = new System.Drawing.Size(425, 22);
      this.txt3.TabIndex = 9;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(3, 68);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(143, 34);
      this.label3.TabIndex = 6;
      this.label3.Text = "EFPTextBox RO";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // br3
      // 
      this.br3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br3.Location = new System.Drawing.Point(583, 70);
      this.br3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br3.Name = "br3";
      this.br3.Size = new System.Drawing.Size(117, 30);
      this.br3.TabIndex = 8;
      this.br3.Text = "Browse";
      this.br3.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(3, 34);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(143, 34);
      this.label2.TabIndex = 3;
      this.label2.Text = "EFPHistComboBox";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // br2
      // 
      this.br2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br2.Location = new System.Drawing.Point(583, 36);
      this.br2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br2.Name = "br2";
      this.br2.Size = new System.Drawing.Size(117, 30);
      this.br2.TabIndex = 5;
      this.br2.Text = "Browse";
      this.br2.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(143, 34);
      this.label1.TabIndex = 0;
      this.label1.Text = "EFPTextBox";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txt1
      // 
      this.txt1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt1.Location = new System.Drawing.Point(152, 2);
      this.txt1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt1.Name = "txt1";
      this.txt1.Size = new System.Drawing.Size(425, 22);
      this.txt1.TabIndex = 1;
      // 
      // br1
      // 
      this.br1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.br1.Location = new System.Drawing.Point(583, 2);
      this.br1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.br1.Name = "br1";
      this.br1.Size = new System.Drawing.Size(117, 30);
      this.br1.TabIndex = 2;
      this.br1.Text = "Browse";
      this.br1.UseVisualStyleBackColor = true;
      // 
      // txt2
      // 
      this.txt2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txt2.FormattingEnabled = true;
      this.txt2.Location = new System.Drawing.Point(152, 36);
      this.txt2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.txt2.Name = "txt2";
      this.txt2.Size = new System.Drawing.Size(425, 24);
      this.txt2.TabIndex = 4;
      // 
      // FileControlsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(709, 389);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.Name = "FileControlsForm";
      this.Text = "EFPFolderBrowserButton и EFPFileDialogButton";
      this.panel1.ResumeLayout(false);
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tableLayoutPanel3.ResumeLayout(false);
      this.tableLayoutPanel3.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button br3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button br2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox txt1;
    private System.Windows.Forms.Button br1;
    private System.Windows.Forms.ComboBox txt2;
    private System.Windows.Forms.TextBox txt3;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.TextBox txt6;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button br6;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Button br5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.TextBox txt4;
    private System.Windows.Forms.Button br4;
    private System.Windows.Forms.ComboBox txt5;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox cbPathValidateMode;
    private System.Windows.Forms.ComboBox cbCanBeEmptyMode;
  }
}

