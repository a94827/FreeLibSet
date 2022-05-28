namespace ManualOrderColumnDemo
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
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.rbEFPInputDataGridView = new System.Windows.Forms.RadioButton();
      this.rbEFPDataGridView = new System.Windows.Forms.RadioButton();
      this.rbEFPDataTreeView = new System.Windows.Forms.RadioButton();
      this.rbBoth = new System.Windows.Forms.RadioButton();
      this.cbReadOnly = new System.Windows.Forms.CheckBox();
      this.cbMultiSelect = new System.Windows.Forms.CheckBox();
      this.cbManualOrderColumn = new System.Windows.Forms.CheckBox();
      this.cbDefaultManualOrderColumn = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbOrderStartMode = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbOrderDataType = new System.Windows.Forms.ComboBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(0, 283);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(513, 58);
      this.infoLabel1.TabIndex = 2;
      this.infoLabel1.Text = "Демонстрирует использование свойства EFPDataGridViewCommandItems, EFPDataTreeView" +
          "CommandItems.ManualOrderColumn и связанных с ним действий";
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 243);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(513, 40);
      this.panel1.TabIndex = 1;
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
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(513, 243);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры";
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(75, 23);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(89, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.rbBoth);
      this.groupBox2.Controls.Add(this.rbEFPDataTreeView);
      this.groupBox2.Controls.Add(this.rbEFPDataGridView);
      this.groupBox2.Controls.Add(this.rbEFPInputDataGridView);
      this.groupBox2.Location = new System.Drawing.Point(12, 19);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(308, 115);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Просмотры";
      // 
      // rbEFPInputDataGridView
      // 
      this.rbEFPInputDataGridView.AutoSize = true;
      this.rbEFPInputDataGridView.Location = new System.Drawing.Point(15, 88);
      this.rbEFPInputDataGridView.Name = "rbEFPInputDataGridView";
      this.rbEFPInputDataGridView.Size = new System.Drawing.Size(134, 17);
      this.rbEFPInputDataGridView.TabIndex = 3;
      this.rbEFPInputDataGridView.TabStop = true;
      this.rbEFPInputDataGridView.Text = "EFPInputDataGridView";
      this.rbEFPInputDataGridView.UseVisualStyleBackColor = true;
      // 
      // rbEFPDataGridView
      // 
      this.rbEFPDataGridView.AutoSize = true;
      this.rbEFPDataGridView.Location = new System.Drawing.Point(15, 19);
      this.rbEFPDataGridView.Name = "rbEFPDataGridView";
      this.rbEFPDataGridView.Size = new System.Drawing.Size(110, 17);
      this.rbEFPDataGridView.TabIndex = 0;
      this.rbEFPDataGridView.TabStop = true;
      this.rbEFPDataGridView.Text = "EFPDataGridView";
      this.rbEFPDataGridView.UseVisualStyleBackColor = true;
      // 
      // rbEFPDataTreeView
      // 
      this.rbEFPDataTreeView.AutoSize = true;
      this.rbEFPDataTreeView.Location = new System.Drawing.Point(15, 42);
      this.rbEFPDataTreeView.Name = "rbEFPDataTreeView";
      this.rbEFPDataTreeView.Size = new System.Drawing.Size(113, 17);
      this.rbEFPDataTreeView.TabIndex = 1;
      this.rbEFPDataTreeView.TabStop = true;
      this.rbEFPDataTreeView.Text = "EFPDataTreeView";
      this.rbEFPDataTreeView.UseVisualStyleBackColor = true;
      // 
      // rbBoth
      // 
      this.rbBoth.AutoSize = true;
      this.rbBoth.Location = new System.Drawing.Point(15, 65);
      this.rbBoth.Name = "rbBoth";
      this.rbBoth.Size = new System.Drawing.Size(204, 17);
      this.rbBoth.TabIndex = 2;
      this.rbBoth.TabStop = true;
      this.rbBoth.Text = "EFPDataGridView+EFPDataTreeView";
      this.rbBoth.UseVisualStyleBackColor = true;
      // 
      // cbReadOnly
      // 
      this.cbReadOnly.AutoSize = true;
      this.cbReadOnly.Location = new System.Drawing.Point(208, 140);
      this.cbReadOnly.Name = "cbReadOnly";
      this.cbReadOnly.Size = new System.Drawing.Size(73, 17);
      this.cbReadOnly.TabIndex = 3;
      this.cbReadOnly.Text = "ReadOnly";
      this.cbReadOnly.UseVisualStyleBackColor = true;
      // 
      // cbMultiSelect
      // 
      this.cbMultiSelect.AutoSize = true;
      this.cbMultiSelect.Location = new System.Drawing.Point(208, 163);
      this.cbMultiSelect.Name = "cbMultiSelect";
      this.cbMultiSelect.Size = new System.Drawing.Size(78, 17);
      this.cbMultiSelect.TabIndex = 4;
      this.cbMultiSelect.Text = "MultiSelect";
      this.cbMultiSelect.UseVisualStyleBackColor = true;
      // 
      // cbManualOrderColumn
      // 
      this.cbManualOrderColumn.AutoSize = true;
      this.cbManualOrderColumn.Location = new System.Drawing.Point(27, 140);
      this.cbManualOrderColumn.Name = "cbManualOrderColumn";
      this.cbManualOrderColumn.Size = new System.Drawing.Size(122, 17);
      this.cbManualOrderColumn.TabIndex = 1;
      this.cbManualOrderColumn.Text = "ManualOrderColumn";
      this.cbManualOrderColumn.UseVisualStyleBackColor = true;
      // 
      // cbDefaultManualOrderColumn
      // 
      this.cbDefaultManualOrderColumn.AutoSize = true;
      this.cbDefaultManualOrderColumn.Location = new System.Drawing.Point(27, 163);
      this.cbDefaultManualOrderColumn.Name = "cbDefaultManualOrderColumn";
      this.cbDefaultManualOrderColumn.Size = new System.Drawing.Size(156, 17);
      this.cbDefaultManualOrderColumn.TabIndex = 2;
      this.cbDefaultManualOrderColumn.Text = "DefaultManualOrderColumn";
      this.cbDefaultManualOrderColumn.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(24, 212);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(170, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "Начальные значения поля Order";
      // 
      // cbOrderStartMode
      // 
      this.cbOrderStartMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbOrderStartMode.FormattingEnabled = true;
      this.cbOrderStartMode.Items.AddRange(new object[] {
            "1,2,3",
            "Нулевые",
            "Случайные"});
      this.cbOrderStartMode.Location = new System.Drawing.Point(208, 209);
      this.cbOrderStartMode.Name = "cbOrderStartMode";
      this.cbOrderStartMode.Size = new System.Drawing.Size(293, 21);
      this.cbOrderStartMode.TabIndex = 8;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(24, 186);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(82, 13);
      this.label2.TabIndex = 5;
      this.label2.Text = "Тип поля Order";
      // 
      // cbOrderDataType
      // 
      this.cbOrderDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbOrderDataType.FormattingEnabled = true;
      this.cbOrderDataType.Items.AddRange(new object[] {
            "Int16",
            "Int32"});
      this.cbOrderDataType.Location = new System.Drawing.Point(208, 183);
      this.cbOrderDataType.Name = "cbOrderDataType";
      this.cbOrderDataType.Size = new System.Drawing.Size(121, 21);
      this.cbOrderDataType.TabIndex = 6;
      // 
      // ParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(513, 341);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.infoLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ParamForm";
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

