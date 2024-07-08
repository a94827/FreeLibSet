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
      this.cbGridProducer = new System.Windows.Forms.CheckBox();
      this.edConfigSectionName = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
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
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(466, 357);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // groupBox4
      // 
      this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox4.Controls.Add(this.cbOLEPreferred);
      this.groupBox4.Location = new System.Drawing.Point(281, 121);
      this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox4.Size = new System.Drawing.Size(183, 194);
      this.groupBox4.TabIndex = 3;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Отладка";
      // 
      // cbOLEPreferred
      // 
      this.cbOLEPreferred.AutoSize = true;
      this.cbOLEPreferred.Location = new System.Drawing.Point(4, 39);
      this.cbOLEPreferred.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbOLEPreferred.Name = "cbOLEPreferred";
      this.cbOLEPreferred.Size = new System.Drawing.Size(90, 17);
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
      this.groupBox3.Location = new System.Drawing.Point(281, 2);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox3.Size = new System.Drawing.Size(183, 115);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Сохранение настроек";
      // 
      // rbDummyConfigManager
      // 
      this.rbDummyConfigManager.AutoSize = true;
      this.rbDummyConfigManager.Location = new System.Drawing.Point(4, 44);
      this.rbDummyConfigManager.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbDummyConfigManager.Name = "rbDummyConfigManager";
      this.rbDummyConfigManager.Size = new System.Drawing.Size(152, 17);
      this.rbDummyConfigManager.TabIndex = 1;
      this.rbDummyConfigManager.TabStop = true;
      this.rbDummyConfigManager.Text = "EFPDummyConfigManager";
      this.rbDummyConfigManager.UseVisualStyleBackColor = true;
      // 
      // rbRegistryConfigManager
      // 
      this.rbRegistryConfigManager.AutoSize = true;
      this.rbRegistryConfigManager.Location = new System.Drawing.Point(4, 22);
      this.rbRegistryConfigManager.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbRegistryConfigManager.Name = "rbRegistryConfigManager";
      this.rbRegistryConfigManager.Size = new System.Drawing.Size(155, 17);
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
      this.groupBox1.Location = new System.Drawing.Point(2, 2);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Size = new System.Drawing.Size(275, 115);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Вариант теста";
      // 
      // rbTest4
      // 
      this.rbTest4.AutoSize = true;
      this.rbTest4.Location = new System.Drawing.Point(14, 88);
      this.rbTest4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbTest4.Name = "rbTest4";
      this.rbTest4.Size = new System.Drawing.Size(110, 17);
      this.rbTest4.TabIndex = 3;
      this.rbTest4.TabStop = true;
      this.rbTest4.Text = "EFPDataGridView";
      this.rbTest4.UseVisualStyleBackColor = true;
      // 
      // rbTest3
      // 
      this.rbTest3.AutoSize = true;
      this.rbTest3.Location = new System.Drawing.Point(14, 66);
      this.rbTest3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbTest3.Name = "rbTest3";
      this.rbTest3.Size = new System.Drawing.Size(184, 17);
      this.rbTest3.TabIndex = 2;
      this.rbTest3.TabStop = true;
      this.rbTest3.Text = "EFPDataTreeView без столбцов";
      this.rbTest3.UseVisualStyleBackColor = true;
      // 
      // rbTest2
      // 
      this.rbTest2.AutoSize = true;
      this.rbTest2.Location = new System.Drawing.Point(14, 44);
      this.rbTest2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbTest2.Name = "rbTest2";
      this.rbTest2.Size = new System.Drawing.Size(186, 17);
      this.rbTest2.TabIndex = 1;
      this.rbTest2.TabStop = true;
      this.rbTest2.Text = "EFPDataTreeView со столбцами";
      this.rbTest2.UseVisualStyleBackColor = true;
      // 
      // rbTest1
      // 
      this.rbTest1.AutoSize = true;
      this.rbTest1.Location = new System.Drawing.Point(14, 22);
      this.rbTest1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.rbTest1.Name = "rbTest1";
      this.rbTest1.Size = new System.Drawing.Size(129, 17);
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
      this.groupBox2.Controls.Add(this.cbGridProducer);
      this.groupBox2.Controls.Add(this.edConfigSectionName);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.cbDefaultConfigs);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cbMultiSelect);
      this.groupBox2.Controls.Add(this.cbAddOutItem);
      this.groupBox2.Controls.Add(this.cbRemoveOutItem);
      this.groupBox2.Controls.Add(this.cbFormatProvider);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Location = new System.Drawing.Point(2, 121);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox2.Size = new System.Drawing.Size(275, 194);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Параметры теста";
      // 
      // cbGridProducer
      // 
      this.cbGridProducer.AutoSize = true;
      this.cbGridProducer.Location = new System.Drawing.Point(14, 76);
      this.cbGridProducer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbGridProducer.Name = "cbGridProducer";
      this.cbGridProducer.Size = new System.Drawing.Size(184, 17);
      this.cbGridProducer.TabIndex = 4;
      this.cbGridProducer.Text = "Использовать EFPGridProducer";
      this.cbGridProducer.UseVisualStyleBackColor = true;
      // 
      // edConfigSectionName
      // 
      this.edConfigSectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edConfigSectionName.Location = new System.Drawing.Point(116, 23);
      this.edConfigSectionName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edConfigSectionName.Name = "edConfigSectionName";
      this.edConfigSectionName.Size = new System.Drawing.Size(157, 20);
      this.edConfigSectionName.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(7, 23);
      this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(104, 19);
      this.label3.TabIndex = 0;
      this.label3.Text = "ConfigSectionName";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
      this.cbDefaultConfigs.Location = new System.Drawing.Point(97, 162);
      this.cbDefaultConfigs.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbDefaultConfigs.Name = "cbDefaultConfigs";
      this.cbDefaultConfigs.Size = new System.Drawing.Size(175, 21);
      this.cbDefaultConfigs.TabIndex = 9;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(7, 162);
      this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(86, 21);
      this.label2.TabIndex = 8;
      this.label2.Text = "DefaultConfigs";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbMultiSelect
      // 
      this.cbMultiSelect.AutoSize = true;
      this.cbMultiSelect.Location = new System.Drawing.Point(14, 139);
      this.cbMultiSelect.Name = "cbMultiSelect";
      this.cbMultiSelect.Size = new System.Drawing.Size(78, 17);
      this.cbMultiSelect.TabIndex = 7;
      this.cbMultiSelect.Text = "MultiSelect";
      this.cbMultiSelect.UseVisualStyleBackColor = true;
      // 
      // cbAddOutItem
      // 
      this.cbAddOutItem.AutoSize = true;
      this.cbAddOutItem.Location = new System.Drawing.Point(14, 117);
      this.cbAddOutItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbAddOutItem.Name = "cbAddOutItem";
      this.cbAddOutItem.Size = new System.Drawing.Size(196, 17);
      this.cbAddOutItem.TabIndex = 6;
      this.cbAddOutItem.Text = "Дополнительные BRMenuOutItem";
      this.cbAddOutItem.UseVisualStyleBackColor = true;
      // 
      // cbRemoveOutItem
      // 
      this.cbRemoveOutItem.AutoSize = true;
      this.cbRemoveOutItem.Location = new System.Drawing.Point(14, 97);
      this.cbRemoveOutItem.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbRemoveOutItem.Name = "cbRemoveOutItem";
      this.cbRemoveOutItem.Size = new System.Drawing.Size(220, 17);
      this.cbRemoveOutItem.TabIndex = 5;
      this.cbRemoveOutItem.Text = "Удалить стандартный BRMenuOutItem";
      this.cbRemoveOutItem.UseVisualStyleBackColor = true;
      // 
      // cbFormatProvider
      // 
      this.cbFormatProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFormatProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFormatProvider.FormattingEnabled = true;
      this.cbFormatProvider.Location = new System.Drawing.Point(116, 48);
      this.cbFormatProvider.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbFormatProvider.Name = "cbFormatProvider";
      this.cbFormatProvider.Size = new System.Drawing.Size(157, 21);
      this.cbFormatProvider.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(7, 48);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 21);
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
      this.panel1.Location = new System.Drawing.Point(2, 319);
      this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(462, 36);
      this.panel1.TabIndex = 4;
      // 
      // lblOSVersion
      // 
      this.lblOSVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblOSVersion.Location = new System.Drawing.Point(99, 6);
      this.lblOSVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.lblOSVersion.Name = "lblOSVersion";
      this.lblOSVersion.Size = new System.Drawing.Size(317, 24);
      this.lblOSVersion.TabIndex = 2;
      this.lblOSVersion.Text = "???";
      this.lblOSVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblOSVersion.UseMnemonic = false;
      // 
      // btnAbout
      // 
      this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAbout.Location = new System.Drawing.Point(421, 6);
      this.btnAbout.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.Size = new System.Drawing.Size(32, 24);
      this.btnAbout.TabIndex = 1;
      this.btnAbout.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Location = new System.Drawing.Point(6, 6);
      this.btnOk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "Тест";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(466, 362);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
    private System.Windows.Forms.CheckBox cbGridProducer;
  }
}
