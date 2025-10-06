namespace WinFormsDemo.EFPDataViewDemo
{
  partial class EFPDataViewParamsForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbReadOnly = new System.Windows.Forms.CheckBox();
      this.cbCanInsert = new System.Windows.Forms.CheckBox();
      this.cbCanInsertCopy = new System.Windows.Forms.CheckBox();
      this.cbCanDelete = new System.Windows.Forms.CheckBox();
      this.cbCanEdit = new System.Windows.Forms.CheckBox();
      this.cbCanView = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbEnterKeyMode = new System.Windows.Forms.ComboBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 274);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(560, 40);
      this.panel1.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.groupBox3);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(560, 274);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbCanView);
      this.groupBox2.Controls.Add(this.cbCanEdit);
      this.groupBox2.Controls.Add(this.cbCanDelete);
      this.groupBox2.Controls.Add(this.cbCanInsertCopy);
      this.groupBox2.Controls.Add(this.cbCanInsert);
      this.groupBox2.Controls.Add(this.cbReadOnly);
      this.groupBox2.Location = new System.Drawing.Point(12, 21);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(159, 186);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      // 
      // cbReadOnly
      // 
      this.cbReadOnly.AutoSize = true;
      this.cbReadOnly.Location = new System.Drawing.Point(17, 21);
      this.cbReadOnly.Name = "cbReadOnly";
      this.cbReadOnly.Size = new System.Drawing.Size(93, 21);
      this.cbReadOnly.TabIndex = 0;
      this.cbReadOnly.Text = "ReadOnly";
      this.cbReadOnly.UseVisualStyleBackColor = true;
      // 
      // cbCanInsert
      // 
      this.cbCanInsert.AutoSize = true;
      this.cbCanInsert.Location = new System.Drawing.Point(17, 48);
      this.cbCanInsert.Name = "cbCanInsert";
      this.cbCanInsert.Size = new System.Drawing.Size(90, 21);
      this.cbCanInsert.TabIndex = 1;
      this.cbCanInsert.Text = "CanInsert";
      this.cbCanInsert.UseVisualStyleBackColor = true;
      // 
      // cbCanInsertCopy
      // 
      this.cbCanInsertCopy.AutoSize = true;
      this.cbCanInsertCopy.Location = new System.Drawing.Point(17, 75);
      this.cbCanInsertCopy.Name = "cbCanInsertCopy";
      this.cbCanInsertCopy.Size = new System.Drawing.Size(122, 21);
      this.cbCanInsertCopy.TabIndex = 2;
      this.cbCanInsertCopy.Text = "CanInsertCopy";
      this.cbCanInsertCopy.UseVisualStyleBackColor = true;
      // 
      // cbCanDelete
      // 
      this.cbCanDelete.AutoSize = true;
      this.cbCanDelete.Location = new System.Drawing.Point(17, 102);
      this.cbCanDelete.Name = "cbCanDelete";
      this.cbCanDelete.Size = new System.Drawing.Size(96, 21);
      this.cbCanDelete.TabIndex = 3;
      this.cbCanDelete.Text = "CanDelete";
      this.cbCanDelete.UseVisualStyleBackColor = true;
      // 
      // cbCanEdit
      // 
      this.cbCanEdit.AutoSize = true;
      this.cbCanEdit.Location = new System.Drawing.Point(17, 129);
      this.cbCanEdit.Name = "cbCanEdit";
      this.cbCanEdit.Size = new System.Drawing.Size(79, 21);
      this.cbCanEdit.TabIndex = 4;
      this.cbCanEdit.Text = "CanEdit";
      this.cbCanEdit.UseVisualStyleBackColor = true;
      // 
      // cbCanView
      // 
      this.cbCanView.AutoSize = true;
      this.cbCanView.Location = new System.Drawing.Point(17, 156);
      this.cbCanView.Name = "cbCanView";
      this.cbCanView.Size = new System.Drawing.Size(84, 21);
      this.cbCanView.TabIndex = 5;
      this.cbCanView.Text = "CanView";
      this.cbCanView.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbEnterKeyMode);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Location = new System.Drawing.Point(12, 213);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(536, 53);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 18);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(119, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "EnterKeyMode";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbEnterKeyMode
      // 
      this.cbEnterKeyMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbEnterKeyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEnterKeyMode.FormattingEnabled = true;
      this.cbEnterKeyMode.Location = new System.Drawing.Point(131, 18);
      this.cbEnterKeyMode.Name = "cbEnterKeyMode";
      this.cbEnterKeyMode.Size = new System.Drawing.Size(388, 24);
      this.cbEnterKeyMode.TabIndex = 1;
      // 
      // EFPDataViewParamsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(560, 314);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Name = "EFPDataViewParamsForm";
      this.Text = "EFPDataGridView & EFPDataTreeView demo";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.ComboBox cbEnterKeyMode;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox cbCanView;
    private System.Windows.Forms.CheckBox cbCanEdit;
    private System.Windows.Forms.CheckBox cbCanDelete;
    private System.Windows.Forms.CheckBox cbCanInsertCopy;
    private System.Windows.Forms.CheckBox cbCanInsert;
    private System.Windows.Forms.CheckBox cbReadOnly;
  }
}