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
      this.cbDefaultConfigs = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbMultiSelect = new System.Windows.Forms.CheckBox();
      this.cbAddOutItem = new System.Windows.Forms.CheckBox();
      this.cbRemoveOutItem = new System.Windows.Forms.CheckBox();
      this.cbFormatProvider = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblOSVersion = new System.Windows.Forms.Label();
      this.btnAbout = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.edConfigSectionName = new System.Windows.Forms.TextBox();
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
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(621, 399);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.cbOLEPreferred);
      this.groupBox4.Location = new System.Drawing.Point(375, 148);
      this.groupBox4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox4.Size = new System.Drawing.Size(243, 203);
      this.groupBox4.TabIndex = 3;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Отладка";
      // 
      // cbOLEPreferred
      // 
      this.cbOLEPreferred.AutoSize = true;
      this.cbOLEPreferred.Location = new System.Drawing.Point(5, 48);
      this.cbOLEPreferred.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.groupBox3.Location = new System.Drawing.Point(375, 2);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox3.Size = new System.Drawing.Size(243, 142);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Сохранение настроек";
      // 
      // rbDummyConfigManager
      // 
      this.rbDummyConfigManager.AutoSize = true;
      this.rbDummyConfigManager.Location = new System.Drawing.Point(5, 54);
      this.rbDummyConfigManager.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.rbRegistryConfigManager.Location = new System.Drawing.Point(5, 27);
      this.rbRegistryConfigManager.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.groupBox1.Location = new System.Drawing.Point(3, 2);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox1.Size = new System.Drawing.Size(366, 142);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Вариант теста";
      // 
      // rbTest4
      // 
      this.rbTest4.AutoSize = true;
      this.rbTest4.Location = new System.Drawing.Point(19, 108);
      this.rbTest4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.rbTest4.Name = "rbTest4";
      this.rbTest4.Size = new System.Drawing.Size(141, 21);
      this.rbTest4.TabIndex = 3;
      this.rbTest4.TabStop = true;
      this.rbTest4.Text = "EFPDataGridView";
      this.rbTest4.UseVisualStyleBackColor = true;
      // 
      // rbTest3
      // 
      this.rbTest3.AutoSize = true;
      this.rbTest3.Location = new System.Drawing.Point(19, 81);
      this.rbTest3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.rbTest2.Location = new System.Drawing.Point(19, 54);
      this.rbTest2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.rbTest1.Location = new System.Drawing.Point(19, 27);
      this.rbTest1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.groupBox2.Controls.Add(this.edConfigSectionName);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.cbDefaultConfigs);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cbMultiSelect);
      this.groupBox2.Controls.Add(this.cbAddOutItem);
      this.groupBox2.Controls.Add(this.cbRemoveOutItem);
      this.groupBox2.Controls.Add(this.cbFormatProvider);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(3, 148);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBox2.Size = new System.Drawing.Size(366, 203);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Параметры теста";
      // 
      // cbDefaultConfigs
      // 
      this.cbDefaultConfigs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDefaultConfigs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDefaultConfigs.FormattingEnabled = true;
      this.cbDefaultConfigs.Items.AddRange(new object[] {
            "Not set",
            "Default config",
            "A4 & A4 landscape"});
      this.cbDefaultConfigs.Location = new System.Drawing.Point(129, 167);
      this.cbDefaultConfigs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbDefaultConfigs.Name = "cbDefaultConfigs";
      this.cbDefaultConfigs.Size = new System.Drawing.Size(231, 24);
      this.cbDefaultConfigs.TabIndex = 8;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(9, 167);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(114, 26);
      this.label2.TabIndex = 7;
      this.label2.Text = "DefaultConfigs";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbMultiSelect
      // 
      this.cbMultiSelect.AutoSize = true;
      this.cbMultiSelect.Location = new System.Drawing.Point(19, 139);
      this.cbMultiSelect.Margin = new System.Windows.Forms.Padding(4);
      this.cbMultiSelect.Name = "cbMultiSelect";
      this.cbMultiSelect.Size = new System.Drawing.Size(98, 21);
      this.cbMultiSelect.TabIndex = 6;
      this.cbMultiSelect.Text = "MultiSelect";
      this.cbMultiSelect.UseVisualStyleBackColor = true;
      // 
      // cbAddOutItem
      // 
      this.cbAddOutItem.AutoSize = true;
      this.cbAddOutItem.Location = new System.Drawing.Point(19, 112);
      this.cbAddOutItem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbAddOutItem.Name = "cbAddOutItem";
      this.cbAddOutItem.Size = new System.Drawing.Size(252, 21);
      this.cbAddOutItem.TabIndex = 5;
      this.cbAddOutItem.Text = "Дополнительные BRMenuOutItem";
      this.cbAddOutItem.UseVisualStyleBackColor = true;
      // 
      // cbRemoveOutItem
      // 
      this.cbRemoveOutItem.AutoSize = true;
      this.cbRemoveOutItem.Location = new System.Drawing.Point(19, 87);
      this.cbRemoveOutItem.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbRemoveOutItem.Name = "cbRemoveOutItem";
      this.cbRemoveOutItem.Size = new System.Drawing.Size(283, 21);
      this.cbRemoveOutItem.TabIndex = 4;
      this.cbRemoveOutItem.Text = "Удалить стандартный BRMenuOutItem";
      this.cbRemoveOutItem.UseVisualStyleBackColor = true;
      // 
      // cbFormatProvider
      // 
      this.cbFormatProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFormatProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFormatProvider.FormattingEnabled = true;
      this.cbFormatProvider.Location = new System.Drawing.Point(154, 59);
      this.cbFormatProvider.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbFormatProvider.Name = "cbFormatProvider";
      this.cbFormatProvider.Size = new System.Drawing.Size(206, 24);
      this.cbFormatProvider.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(9, 59);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(139, 26);
      this.label1.TabIndex = 2;
      this.label1.Text = "FormatProvider";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.panel1.Location = new System.Drawing.Point(3, 355);
      this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(615, 42);
      this.panel1.TabIndex = 4;
      // 
      // lblOSVersion
      // 
      this.lblOSVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOSVersion.Location = new System.Drawing.Point(115, 0);
      this.lblOSVersion.Name = "lblOSVersion";
      this.lblOSVersion.Size = new System.Drawing.Size(450, 45);
      this.lblOSVersion.TabIndex = 2;
      this.lblOSVersion.Text = "???";
      this.lblOSVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblOSVersion.UseMnemonic = false;
      // 
      // btnAbout
      // 
      this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAbout.Location = new System.Drawing.Point(574, 7);
      this.btnAbout.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.Size = new System.Drawing.Size(32, 25);
      this.btnAbout.TabIndex = 1;
      this.btnAbout.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(8, 7);
      this.btnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 25);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "Тест";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(9, 28);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(139, 23);
      this.label3.TabIndex = 0;
      this.label3.Text = "ConfigSectionName";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edConfigSectionName
      // 
      this.edConfigSectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edConfigSectionName.Location = new System.Drawing.Point(154, 28);
      this.edConfigSectionName.Name = "edConfigSectionName";
      this.edConfigSectionName.Size = new System.Drawing.Size(206, 22);
      this.edConfigSectionName.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(621, 404);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
    private System.Windows.Forms.CheckBox cbAddOutItem;
    private System.Windows.Forms.CheckBox cbRemoveOutItem;
    private System.Windows.Forms.CheckBox cbMultiSelect;
    private System.Windows.Forms.ComboBox cbDefaultConfigs;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox edConfigSectionName;
    private System.Windows.Forms.Label label3;
  }
}
