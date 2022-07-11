namespace FreeLibSet.Forms
{
  partial class EFPDataGridViewExpExcelForm
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
      this.grpUse = new System.Windows.Forms.GroupBox();
      this.cbUseBorders = new System.Windows.Forms.CheckBox();
      this.cbUseFill = new System.Windows.Forms.CheckBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.grpArea = new System.Windows.Forms.GroupBox();
      this.cbHeaders = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbAll = new System.Windows.Forms.RadioButton();
      this.rbSelected = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbBoolMode = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.grpUse.SuspendLayout();
      this.grpArea.SuspendLayout();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpUse
      // 
      this.grpUse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpUse.Controls.Add(this.cbUseBorders);
      this.grpUse.Controls.Add(this.cbUseFill);
      this.grpUse.Location = new System.Drawing.Point(12, 113);
      this.grpUse.Name = "grpUse";
      this.grpUse.Size = new System.Drawing.Size(399, 63);
      this.grpUse.TabIndex = 2;
      this.grpUse.TabStop = false;
      this.grpUse.Text = "Применить оформление";
      // 
      // cbUseBorders
      // 
      this.cbUseBorders.AutoSize = true;
      this.cbUseBorders.Location = new System.Drawing.Point(17, 42);
      this.cbUseBorders.Name = "cbUseBorders";
      this.cbUseBorders.Size = new System.Drawing.Size(70, 17);
      this.cbUseBorders.TabIndex = 1;
      this.cbUseBorders.Text = "Границы";
      this.cbUseBorders.UseVisualStyleBackColor = true;
      // 
      // cbUseFill
      // 
      this.cbUseFill.AutoSize = true;
      this.cbUseFill.Location = new System.Drawing.Point(17, 19);
      this.cbUseFill.Name = "cbUseFill";
      this.cbUseFill.Size = new System.Drawing.Size(51, 17);
      this.cbUseFill.TabIndex = 0;
      this.cbUseFill.Text = "Цвет";
      this.cbUseFill.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(427, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 3;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(427, 42);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 4;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // grpArea
      // 
      this.grpArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpArea.Controls.Add(this.cbHeaders);
      this.grpArea.Controls.Add(this.panel1);
      this.grpArea.Location = new System.Drawing.Point(12, 12);
      this.grpArea.Name = "grpArea";
      this.grpArea.Size = new System.Drawing.Size(399, 95);
      this.grpArea.TabIndex = 1;
      this.grpArea.TabStop = false;
      this.grpArea.Text = "Область";
      // 
      // cbHeaders
      // 
      this.cbHeaders.AutoSize = true;
      this.cbHeaders.Location = new System.Drawing.Point(17, 70);
      this.cbHeaders.Name = "cbHeaders";
      this.cbHeaders.Size = new System.Drawing.Size(130, 17);
      this.cbHeaders.TabIndex = 1;
      this.cbHeaders.Text = "Заголовки столбцов";
      this.cbHeaders.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.rbAll);
      this.panel1.Controls.Add(this.rbSelected);
      this.panel1.Location = new System.Drawing.Point(14, 19);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(200, 43);
      this.panel1.TabIndex = 0;
      // 
      // rbAll
      // 
      this.rbAll.AutoSize = true;
      this.rbAll.Location = new System.Drawing.Point(3, 0);
      this.rbAll.Name = "rbAll";
      this.rbAll.Size = new System.Drawing.Size(88, 17);
      this.rbAll.TabIndex = 0;
      this.rbAll.TabStop = true;
      this.rbAll.Text = "Вся таблица";
      this.rbAll.UseVisualStyleBackColor = true;
      // 
      // rbSelected
      // 
      this.rbSelected.AutoSize = true;
      this.rbSelected.Location = new System.Drawing.Point(3, 23);
      this.rbSelected.Name = "rbSelected";
      this.rbSelected.Size = new System.Drawing.Size(122, 17);
      this.rbSelected.TabIndex = 1;
      this.rbSelected.TabStop = true;
      this.rbSelected.Text = "Выбранные ячейки";
      this.rbSelected.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.cbBoolMode);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(12, 182);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(399, 46);
      this.groupBox1.TabIndex = 5;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Замена";
      // 
      // cbBoolMode
      // 
      this.cbBoolMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbBoolMode.FormattingEnabled = true;
      this.cbBoolMode.Items.AddRange(new object[] {
            "Не заменять (ИСТИНА, ЛОЖЬ)",
            "Как число (1, 0)",
            "Как текст ([X], [ ])"});
      this.cbBoolMode.Location = new System.Drawing.Point(162, 16);
      this.cbBoolMode.Name = "cbBoolMode";
      this.cbBoolMode.Size = new System.Drawing.Size(225, 21);
      this.cbBoolMode.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Логические значения";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // EFPDataGridViewExpExcelForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(527, 233);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.grpArea);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.grpUse);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPDataGridViewExpExcelForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.grpUse.ResumeLayout(false);
      this.grpUse.PerformLayout();
      this.grpArea.ResumeLayout(false);
      this.grpArea.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpUse;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox cbUseBorders;
    private System.Windows.Forms.CheckBox cbUseFill;
    private System.Windows.Forms.GroupBox grpArea;
    private System.Windows.Forms.RadioButton rbSelected;
    private System.Windows.Forms.RadioButton rbAll;
    private System.Windows.Forms.CheckBox cbHeaders;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbBoolMode;
    private System.Windows.Forms.Label label1;
  }
}