namespace FreeLibSet.Forms.Data
{
  partial class CodeGridFilterForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbExclude = new System.Windows.Forms.RadioButton();
      this.rbInclude = new System.Windows.Forms.RadioButton();
      this.rbNoFilter = new System.Windows.Forms.RadioButton();
      this.CodesGroupBox = new System.Windows.Forms.GroupBox();
      this.cbEmpty = new System.Windows.Forms.CheckBox();
      this.edCodes = new FreeLibSet.Controls.UserTextComboBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.CodesGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbExclude);
      this.groupBox1.Controls.Add(this.rbInclude);
      this.groupBox1.Controls.Add(this.rbNoFilter);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(160, 86);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Режим фильтра";
      // 
      // rbExclude
      // 
      this.rbExclude.AutoSize = true;
      this.rbExclude.Location = new System.Drawing.Point(6, 64);
      this.rbExclude.Name = "rbExclude";
      this.rbExclude.Size = new System.Drawing.Size(110, 17);
      this.rbExclude.TabIndex = 2;
      this.rbExclude.TabStop = true;
      this.rbExclude.Text = "&Исключить коды";
      this.rbExclude.UseVisualStyleBackColor = true;
      // 
      // rbInclude
      // 
      this.rbInclude.AutoSize = true;
      this.rbInclude.Location = new System.Drawing.Point(6, 42);
      this.rbInclude.Name = "rbInclude";
      this.rbInclude.Size = new System.Drawing.Size(103, 17);
      this.rbInclude.TabIndex = 1;
      this.rbInclude.TabStop = true;
      this.rbInclude.Text = "&Включить коды";
      this.rbInclude.UseVisualStyleBackColor = true;
      // 
      // rbNoFilter
      // 
      this.rbNoFilter.AutoSize = true;
      this.rbNoFilter.Location = new System.Drawing.Point(6, 19);
      this.rbNoFilter.Name = "rbNoFilter";
      this.rbNoFilter.Size = new System.Drawing.Size(90, 17);
      this.rbNoFilter.TabIndex = 0;
      this.rbNoFilter.TabStop = true;
      this.rbNoFilter.Text = "&Нет фильтра";
      this.rbNoFilter.UseVisualStyleBackColor = true;
      // 
      // CodesGroupBox
      // 
      this.CodesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.CodesGroupBox.Controls.Add(this.label1);
      this.CodesGroupBox.Controls.Add(this.cbEmpty);
      this.CodesGroupBox.Controls.Add(this.edCodes);
      this.CodesGroupBox.Location = new System.Drawing.Point(12, 104);
      this.CodesGroupBox.Name = "CodesGroupBox";
      this.CodesGroupBox.Size = new System.Drawing.Size(573, 83);
      this.CodesGroupBox.TabIndex = 1;
      this.CodesGroupBox.TabStop = false;
      // 
      // cbEmpty
      // 
      this.cbEmpty.AutoSize = true;
      this.cbEmpty.Location = new System.Drawing.Point(6, 59);
      this.cbEmpty.Name = "cbEmpty";
      this.cbEmpty.Size = new System.Drawing.Size(114, 17);
      this.cbEmpty.TabIndex = 2;
      this.cbEmpty.Text = "или Код &не задан";
      this.cbEmpty.UseVisualStyleBackColor = true;
      // 
      // edCodes
      // 
      this.edCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edCodes.ClearButtonEnabled = true;
      this.edCodes.EditButtonEnabled = true;
      this.edCodes.Location = new System.Drawing.Point(6, 32);
      this.edCodes.Name = "edCodes";
      this.edCodes.Size = new System.Drawing.Size(561, 21);
      this.edCodes.TabIndex = 1;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(497, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 2;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(497, 42);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 3;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(34, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Ко&ды";
      // 
      // CodeFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(597, 192);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.CodesGroupBox);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CodeFilterForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.CodesGroupBox.ResumeLayout(false);
      this.CodesGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbExclude;
    private System.Windows.Forms.RadioButton rbInclude;
    private System.Windows.Forms.RadioButton rbNoFilter;
    private System.Windows.Forms.GroupBox CodesGroupBox;
    private System.Windows.Forms.CheckBox cbEmpty;
    private FreeLibSet.Controls.UserTextComboBox edCodes;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label label1;
  }
}
