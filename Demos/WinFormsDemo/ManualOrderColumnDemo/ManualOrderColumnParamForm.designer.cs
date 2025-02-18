namespace WinFormsDemo.ManualOrderColumnDemo
{
  partial class ManualOrderColumnParamForm
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
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbOrderDataType = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbOrderStartMode = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbDefaultManualOrderColumn = new System.Windows.Forms.CheckBox();
      this.cbManualOrderColumn = new System.Windows.Forms.CheckBox();
      this.cbMultiSelect = new System.Windows.Forms.CheckBox();
      this.cbReadOnly = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.rbBoth = new System.Windows.Forms.RadioButton();
      this.rbEFPDataTreeView = new System.Windows.Forms.RadioButton();
      this.rbEFPDataGridView = new System.Windows.Forms.RadioButton();
      this.rbEFPInputDataGridView = new System.Windows.Forms.RadioButton();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(0, 349);
      this.infoLabel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(684, 71);
      this.infoLabel1.TabIndex = 2;
      this.infoLabel1.Text = "This demo shows the usage of properties EFPDataGridViewCommandItems, EFPDataTreeV" +
    "iewCommandItems.ManualOrderColumn & related actions";
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 300);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(684, 49);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(119, 10);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
      this.btnOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(100, 28);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "O&K";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbOrderDataType);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbOrderStartMode);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbDefaultManualOrderColumn);
      this.groupBox1.Controls.Add(this.cbManualOrderColumn);
      this.groupBox1.Controls.Add(this.cbMultiSelect);
      this.groupBox1.Controls.Add(this.cbReadOnly);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Size = new System.Drawing.Size(684, 300);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Parameters";
      // 
      // cbOrderDataType
      // 
      this.cbOrderDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbOrderDataType.FormattingEnabled = true;
      this.cbOrderDataType.Items.AddRange(new object[] {
            "Int16",
            "Int32"});
      this.cbOrderDataType.Location = new System.Drawing.Point(277, 225);
      this.cbOrderDataType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbOrderDataType.Name = "cbOrderDataType";
      this.cbOrderDataType.Size = new System.Drawing.Size(160, 24);
      this.cbOrderDataType.TabIndex = 6;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(32, 229);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(125, 17);
      this.label2.TabIndex = 5;
      this.label2.Text = "Order column type";
      // 
      // cbOrderStartMode
      // 
      this.cbOrderStartMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbOrderStartMode.FormattingEnabled = true;
      this.cbOrderStartMode.Items.AddRange(new object[] {
            "1,2,3",
            "Нулевые",
            "Случайные"});
      this.cbOrderStartMode.Location = new System.Drawing.Point(277, 257);
      this.cbOrderStartMode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbOrderStartMode.Name = "cbOrderStartMode";
      this.cbOrderStartMode.Size = new System.Drawing.Size(389, 24);
      this.cbOrderStartMode.TabIndex = 8;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(32, 261);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(213, 17);
      this.label1.TabIndex = 7;
      this.label1.Text = "Initial value for the Order column";
      // 
      // cbDefaultManualOrderColumn
      // 
      this.cbDefaultManualOrderColumn.AutoSize = true;
      this.cbDefaultManualOrderColumn.Location = new System.Drawing.Point(36, 201);
      this.cbDefaultManualOrderColumn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbDefaultManualOrderColumn.Name = "cbDefaultManualOrderColumn";
      this.cbDefaultManualOrderColumn.Size = new System.Drawing.Size(205, 21);
      this.cbDefaultManualOrderColumn.TabIndex = 2;
      this.cbDefaultManualOrderColumn.Text = "DefaultManualOrderColumn";
      this.cbDefaultManualOrderColumn.UseVisualStyleBackColor = true;
      // 
      // cbManualOrderColumn
      // 
      this.cbManualOrderColumn.AutoSize = true;
      this.cbManualOrderColumn.Location = new System.Drawing.Point(36, 172);
      this.cbManualOrderColumn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbManualOrderColumn.Name = "cbManualOrderColumn";
      this.cbManualOrderColumn.Size = new System.Drawing.Size(160, 21);
      this.cbManualOrderColumn.TabIndex = 1;
      this.cbManualOrderColumn.Text = "ManualOrderColumn";
      this.cbManualOrderColumn.UseVisualStyleBackColor = true;
      // 
      // cbMultiSelect
      // 
      this.cbMultiSelect.AutoSize = true;
      this.cbMultiSelect.Location = new System.Drawing.Point(277, 201);
      this.cbMultiSelect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbMultiSelect.Name = "cbMultiSelect";
      this.cbMultiSelect.Size = new System.Drawing.Size(98, 21);
      this.cbMultiSelect.TabIndex = 4;
      this.cbMultiSelect.Text = "MultiSelect";
      this.cbMultiSelect.UseVisualStyleBackColor = true;
      // 
      // cbReadOnly
      // 
      this.cbReadOnly.AutoSize = true;
      this.cbReadOnly.Location = new System.Drawing.Point(277, 172);
      this.cbReadOnly.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbReadOnly.Name = "cbReadOnly";
      this.cbReadOnly.Size = new System.Drawing.Size(93, 21);
      this.cbReadOnly.TabIndex = 3;
      this.cbReadOnly.Text = "ReadOnly";
      this.cbReadOnly.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.rbBoth);
      this.groupBox2.Controls.Add(this.rbEFPDataTreeView);
      this.groupBox2.Controls.Add(this.rbEFPDataGridView);
      this.groupBox2.Controls.Add(this.rbEFPInputDataGridView);
      this.groupBox2.Location = new System.Drawing.Point(16, 23);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Size = new System.Drawing.Size(411, 142);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Data views";
      // 
      // rbBoth
      // 
      this.rbBoth.AutoSize = true;
      this.rbBoth.Location = new System.Drawing.Point(20, 80);
      this.rbBoth.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbBoth.Name = "rbBoth";
      this.rbBoth.Size = new System.Drawing.Size(264, 21);
      this.rbBoth.TabIndex = 2;
      this.rbBoth.TabStop = true;
      this.rbBoth.Text = "EFPDataGridView+EFPDataTreeView";
      this.rbBoth.UseVisualStyleBackColor = true;
      // 
      // rbEFPDataTreeView
      // 
      this.rbEFPDataTreeView.AutoSize = true;
      this.rbEFPDataTreeView.Location = new System.Drawing.Point(20, 52);
      this.rbEFPDataTreeView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbEFPDataTreeView.Name = "rbEFPDataTreeView";
      this.rbEFPDataTreeView.Size = new System.Drawing.Size(144, 21);
      this.rbEFPDataTreeView.TabIndex = 1;
      this.rbEFPDataTreeView.TabStop = true;
      this.rbEFPDataTreeView.Text = "EFPDataTreeView";
      this.rbEFPDataTreeView.UseVisualStyleBackColor = true;
      // 
      // rbEFPDataGridView
      // 
      this.rbEFPDataGridView.AutoSize = true;
      this.rbEFPDataGridView.Location = new System.Drawing.Point(20, 23);
      this.rbEFPDataGridView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbEFPDataGridView.Name = "rbEFPDataGridView";
      this.rbEFPDataGridView.Size = new System.Drawing.Size(141, 21);
      this.rbEFPDataGridView.TabIndex = 0;
      this.rbEFPDataGridView.TabStop = true;
      this.rbEFPDataGridView.Text = "EFPDataGridView";
      this.rbEFPDataGridView.UseVisualStyleBackColor = true;
      // 
      // rbEFPInputDataGridView
      // 
      this.rbEFPInputDataGridView.AutoSize = true;
      this.rbEFPInputDataGridView.Location = new System.Drawing.Point(20, 108);
      this.rbEFPInputDataGridView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.rbEFPInputDataGridView.Name = "rbEFPInputDataGridView";
      this.rbEFPInputDataGridView.Size = new System.Drawing.Size(172, 21);
      this.rbEFPInputDataGridView.TabIndex = 3;
      this.rbEFPInputDataGridView.TabStop = true;
      this.rbEFPInputDataGridView.Text = "EFPInputDataGridView";
      this.rbEFPInputDataGridView.UseVisualStyleBackColor = true;
      // 
      // ManualOrderColumnParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(684, 420);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.infoLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ManualOrderColumnParamForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "ManualOrderColumn demo";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private FreeLibSet.Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.RadioButton rbBoth;
    private System.Windows.Forms.RadioButton rbEFPDataTreeView;
    private System.Windows.Forms.RadioButton rbEFPDataGridView;
    private System.Windows.Forms.RadioButton rbEFPInputDataGridView;
    private System.Windows.Forms.CheckBox cbReadOnly;
    private System.Windows.Forms.CheckBox cbMultiSelect;
    private System.Windows.Forms.CheckBox cbManualOrderColumn;
    private System.Windows.Forms.CheckBox cbDefaultManualOrderColumn;
    private System.Windows.Forms.ComboBox cbOrderStartMode;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbOrderDataType;
  }
}

