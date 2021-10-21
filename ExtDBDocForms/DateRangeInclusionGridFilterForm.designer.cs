namespace FreeLibSet.Forms.Docs
{
  partial class DateRangeInclusionGridFilterForm
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
      this.label2 = new System.Windows.Forms.Label();
      this.edDate = new FreeLibSet.Controls.DateBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbSet = new System.Windows.Forms.RadioButton();
      this.rbWorkDate = new System.Windows.Forms.RadioButton();
      this.rbNone = new System.Windows.Forms.RadioButton();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.groupBox1.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.infoLabel1);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.edDate);
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(361, 149);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Фильтр по интервалу дат";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(152, 65);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(57, 21);
      this.label2.TabIndex = 1;
      this.label2.Text = "&Дата";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDate
      // 
      this.edDate.Location = new System.Drawing.Point(212, 65);
      this.edDate.Margin = new System.Windows.Forms.Padding(0);
      this.edDate.Name = "edDate";
      this.edDate.Size = new System.Drawing.Size(143, 20);
      this.edDate.TabIndex = 2;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.rbSet);
      this.panel1.Controls.Add(this.rbWorkDate);
      this.panel1.Controls.Add(this.rbNone);
      this.panel1.Location = new System.Drawing.Point(6, 19);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(125, 73);
      this.panel1.TabIndex = 0;
      // 
      // rbSet
      // 
      this.rbSet.AutoSize = true;
      this.rbSet.Location = new System.Drawing.Point(3, 50);
      this.rbSet.Name = "rbSet";
      this.rbSet.Size = new System.Drawing.Size(61, 17);
      this.rbSet.TabIndex = 2;
      this.rbSet.TabStop = true;
      this.rbSet.Text = "&Задать";
      this.rbSet.UseVisualStyleBackColor = true;
      // 
      // rbWorkDate
      // 
      this.rbWorkDate.AutoSize = true;
      this.rbWorkDate.Location = new System.Drawing.Point(3, 27);
      this.rbWorkDate.Name = "rbWorkDate";
      this.rbWorkDate.Size = new System.Drawing.Size(96, 17);
      this.rbWorkDate.TabIndex = 1;
      this.rbWorkDate.TabStop = true;
      this.rbWorkDate.Text = "Текущая дата";
      this.rbWorkDate.UseVisualStyleBackColor = true;
      // 
      // rbNone
      // 
      this.rbNone.AutoSize = true;
      this.rbNone.Location = new System.Drawing.Point(3, 4);
      this.rbNone.Name = "rbNone";
      this.rbNone.Size = new System.Drawing.Size(90, 17);
      this.rbNone.TabIndex = 0;
      this.rbNone.TabStop = true;
      this.rbNone.Text = "&Нет фильтра";
      this.rbNone.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(384, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(384, 42);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(3, 98);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(355, 48);
      this.infoLabel1.TabIndex = 3;
      this.infoLabel1.Text = "Если фильтр установлен, то будут отобраны записи, в которых интервал дат включает" +
          " в себя указанную в фильтре дату";
      // 
      // GridFilterDateRangeForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(479, 173);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Name = "GridFilterDateRangeForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.groupBox1.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private FreeLibSet.Controls.DateBox edDate;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbSet;
    private System.Windows.Forms.RadioButton rbWorkDate;
    private System.Windows.Forms.RadioButton rbNone;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.InfoLabel infoLabel1;
  }
}