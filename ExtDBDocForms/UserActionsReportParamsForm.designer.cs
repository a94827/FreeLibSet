namespace AgeyevAV.ExtForms.Docs
{
  partial class UserActionsReportParamsForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbOneType = new System.Windows.Forms.CheckBox();
      this.cbDocType = new System.Windows.Forms.ComboBox();
      this.cbUser = new AgeyevAV.ExtForms.UserSelComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnLastDay = new System.Windows.Forms.Button();
      this.edPeriod = new AgeyevAV.ExtForms.DateRangeBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 147);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(430, 37);
      this.panel1.TabIndex = 1;
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
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.groupBox3);
      this.panel2.Controls.Add(this.groupBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(430, 147);
      this.panel2.TabIndex = 0;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbOneType);
      this.groupBox3.Controls.Add(this.cbDocType);
      this.groupBox3.Controls.Add(this.cbUser);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Location = new System.Drawing.Point(0, 65);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(430, 82);
      this.groupBox3.TabIndex = 6;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Исходные данные";
      // 
      // cbOneType
      // 
      this.cbOneType.AutoSize = true;
      this.cbOneType.Location = new System.Drawing.Point(9, 55);
      this.cbOneType.Name = "cbOneType";
      this.cbOneType.Size = new System.Drawing.Size(135, 17);
      this.cbOneType.TabIndex = 2;
      this.cbOneType.Text = "Один &тип документов";
      this.cbOneType.UseVisualStyleBackColor = true;
      // 
      // cbDocType
      // 
      this.cbDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDocType.FormattingEnabled = true;
      this.cbDocType.Location = new System.Drawing.Point(158, 51);
      this.cbDocType.Name = "cbDocType";
      this.cbDocType.Size = new System.Drawing.Size(262, 21);
      this.cbDocType.TabIndex = 3;
      // 
      // cbUser
      // 
      this.cbUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbUser.Location = new System.Drawing.Point(158, 20);
      this.cbUser.Name = "cbUser";
      this.cbUser.Size = new System.Drawing.Size(259, 20);
      this.cbUser.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(146, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Пользователь";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnLastDay);
      this.groupBox1.Controls.Add(this.edPeriod);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(430, 65);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Период";
      // 
      // btnLastDay
      // 
      this.btnLastDay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLastDay.Location = new System.Drawing.Point(385, 16);
      this.btnLastDay.Name = "btnLastDay";
      this.btnLastDay.Size = new System.Drawing.Size(32, 24);
      this.btnLastDay.TabIndex = 1;
      this.btnLastDay.UseVisualStyleBackColor = true;
      // 
      // edPeriod
      // 
      this.edPeriod.Location = new System.Drawing.Point(6, 19);
      this.edPeriod.Name = "edPeriod";
      this.edPeriod.Size = new System.Drawing.Size(367, 37);
      this.edPeriod.TabIndex = 0;
      // 
      // ShowUserActionsParamsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(430, 184);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ShowUserActionsParamsForm";
      this.Text = "Действия пользователя";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox groupBox1;
    private DateRangeBox edPeriod;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox cbOneType;
    private System.Windows.Forms.ComboBox cbDocType;
    public UserSelComboBox cbUser;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnLastDay;
  }
}