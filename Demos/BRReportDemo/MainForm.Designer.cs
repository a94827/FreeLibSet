namespace BRReportDemo
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.cbOLEPreferred = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.rbDummyConfigManager = new System.Windows.Forms.RadioButton();
      this.rbRegistryConfigManager = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbTest4 = new System.Windows.Forms.RadioButton();
      this.rbTest3 = new System.Windows.Forms.RadioButton();
      this.rbTest2 = new System.Windows.Forms.RadioButton();
      this.rbTest1 = new System.Windows.Forms.RadioButton();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbFormatProvider = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnAbout = new System.Windows.Forms.Button();
      this.lblOSVersion = new System.Windows.Forms.Label();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
      this.tableLayoutPanel1.Controls.Add(this.groupBox4, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.groupBox3, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(622, 342);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.cbOLEPreferred);
      this.groupBox4.Location = new System.Drawing.Point(376, 151);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(243, 142);
      this.groupBox4.TabIndex = 3;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Отладка";
      // 
      // cbOLEPreferred
      // 
      this.cbOLEPreferred.AutoSize = true;
      this.cbOLEPreferred.Location = new System.Drawing.Point(6, 48);
      this.cbOLEPreferred.Name = "cbOLEPreferred";
      this.cbOLEPreferred.Size = new System.Drawing.Size(118, 21);
      this.cbOLEPreferred.TabIndex = 0;
      this.cbOLEPreferred.Text = "OLEPreferred";
      this.cbOLEPreferred.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.rbDummyConfigManager);
      this.groupBox3.Controls.Add(this.rbRegistryConfigManager);
      this.groupBox3.Location = new System.Drawing.Point(376, 3);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(243, 142);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Сохранение настроек";
      // 
      // rbDummyConfigManager
      // 
      this.rbDummyConfigManager.AutoSize = true;
      this.rbDummyConfigManager.Location = new System.Drawing.Point(6, 54);
      this.rbDummyConfigManager.Name = "rbDummyConfigManager";
      this.rbDummyConfigManager.Size = new System.Drawing.Size(198, 21);
      this.rbDummyConfigManager.TabIndex = 1;
      this.rbDummyConfigManager.TabStop = true;
      this.rbDummyConfigManager.Text = "EFPDummyConfigManager";
      this.rbDummyConfigManager.UseVisualStyleBackColor = true;
      // 
      // rbRegistryConfigManager
      // 
      this.rbRegistryConfigManager.AutoSize = true;
      this.rbRegistryConfigManager.Location = new System.Drawing.Point(6, 27);
      this.rbRegistryConfigManager.Name = "rbRegistryConfigManager";
      this.rbRegistryConfigManager.Size = new System.Drawing.Size(203, 21);
      this.rbRegistryConfigManager.TabIndex = 0;
      this.rbRegistryConfigManager.TabStop = true;
      this.rbRegistryConfigManager.Text = "EFPRegistryConfigManager";
      this.rbRegistryConfigManager.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.rbTest4);
      this.groupBox1.Controls.Add(this.rbTest3);
      this.groupBox1.Controls.Add(this.rbTest2);
      this.groupBox1.Controls.Add(this.rbTest1);
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(367, 142);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Вариант теста";
      // 
      // rbTest4
      // 
      this.rbTest4.AutoSize = true;
      this.rbTest4.Location = new System.Drawing.Point(18, 108);
      this.rbTest4.Name = "rbTest4";
      this.rbTest4.Size = new System.Drawing.Size(294, 21);
      this.rbTest4.TabIndex = 3;
      this.rbTest4.TabStop = true;
      this.rbTest4.Text = "EFPDataTreeView с доп. BRMenuOutItem";
      this.rbTest4.UseVisualStyleBackColor = true;
      // 
      // rbTest3
      // 
      this.rbTest3.AutoSize = true;
      this.rbTest3.Location = new System.Drawing.Point(18, 81);
      this.rbTest3.Name = "rbTest3";
      this.rbTest3.Size = new System.Drawing.Size(236, 21);
      this.rbTest3.TabIndex = 2;
      this.rbTest3.TabStop = true;
      this.rbTest3.Text = "EFPDataTreeView без столбцов";
      this.rbTest3.UseVisualStyleBackColor = true;
      // 
      // rbTest2
      // 
      this.rbTest2.AutoSize = true;
      this.rbTest2.Location = new System.Drawing.Point(18, 54);
      this.rbTest2.Name = "rbTest2";
      this.rbTest2.Size = new System.Drawing.Size(238, 21);
      this.rbTest2.TabIndex = 1;
      this.rbTest2.TabStop = true;
      this.rbTest2.Text = "EFPDataTreeView со столбцами";
      this.rbTest2.UseVisualStyleBackColor = true;
      // 
      // rbTest1
      // 
      this.rbTest1.AutoSize = true;
      this.rbTest1.Location = new System.Drawing.Point(18, 27);
      this.rbTest1.Name = "rbTest1";
      this.rbTest1.Size = new System.Drawing.Size(166, 21);
      this.rbTest1.TabIndex = 0;
      this.rbTest1.TabStop = true;
      this.rbTest1.Text = "BRPrintPreviewDialog";
      this.rbTest1.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.cbFormatProvider);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(3, 151);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(367, 142);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Параметры теста";
      // 
      // cbFormatProvider
      // 
      this.cbFormatProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFormatProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFormatProvider.FormattingEnabled = true;
      this.cbFormatProvider.Location = new System.Drawing.Point(9, 48);
      this.cbFormatProvider.Name = "cbFormatProvider";
      this.cbFormatProvider.Size = new System.Drawing.Size(352, 24);
      this.cbFormatProvider.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(105, 17);
      this.label1.TabIndex = 0;
      this.label1.Text = "FormatProvider";
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
      this.panel1.Controls.Add(this.lblOSVersion);
      this.panel1.Controls.Add(this.btnAbout);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Location = new System.Drawing.Point(3, 299);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(616, 40);
      this.panel1.TabIndex = 4;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "Тест";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnAbout
      // 
      this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAbout.Location = new System.Drawing.Point(575, 8);
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.Size = new System.Drawing.Size(32, 24);
      this.btnAbout.TabIndex = 1;
      this.btnAbout.UseVisualStyleBackColor = true;
      // 
      // lblOSVersion
      // 
      this.lblOSVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOSVersion.Location = new System.Drawing.Point(115, 0);
      this.lblOSVersion.Name = "lblOSVersion";
      this.lblOSVersion.Size = new System.Drawing.Size(450, 43);
      this.lblOSVersion.TabIndex = 2;
      this.lblOSVersion.Text = "???";
      this.lblOSVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblOSVersion.UseMnemonic = false;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(622, 338);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.Text = "BRReportDemo";
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.RadioButton rbTest4;
    private System.Windows.Forms.RadioButton rbTest3;
    private System.Windows.Forms.RadioButton rbTest2;
    private System.Windows.Forms.RadioButton rbTest1;
    private System.Windows.Forms.RadioButton rbDummyConfigManager;
    private System.Windows.Forms.RadioButton rbRegistryConfigManager;
    private System.Windows.Forms.ComboBox cbFormatProvider;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox cbOLEPreferred;
    private System.Windows.Forms.Label lblOSVersion;
    private System.Windows.Forms.Button btnAbout;
  }
}