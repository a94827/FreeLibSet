namespace FreeLibSet.Forms.FIAS
{
  partial class FiasSearchForm
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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbText = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.edStartAddress = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbStreet = new System.Windows.Forms.CheckBox();
      this.cbPlanningStructure = new System.Windows.Forms.CheckBox();
      this.cbVillage = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbActual = new System.Windows.Forms.CheckBox();
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
      this.panel1.Location = new System.Drawing.Point(0, 214);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(545, 40);
      this.panel1.TabIndex = 3;
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
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbText);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(545, 59);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Что искать";
      // 
      // cbText
      // 
      this.cbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbText.FormattingEnabled = true;
      this.cbText.Location = new System.Drawing.Point(128, 23);
      this.cbText.Name = "cbText";
      this.cbText.Size = new System.Drawing.Size(405, 21);
      this.cbText.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(22, 23);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "&Текст";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.edStartAddress);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cbStreet);
      this.groupBox2.Controls.Add(this.cbPlanningStructure);
      this.groupBox2.Controls.Add(this.cbVillage);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 59);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(545, 100);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Где искать";
      // 
      // edStartAddress
      // 
      this.edStartAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edStartAddress.Location = new System.Drawing.Point(241, 40);
      this.edStartAddress.Multiline = true;
      this.edStartAddress.Name = "edStartAddress";
      this.edStartAddress.ReadOnly = true;
      this.edStartAddress.Size = new System.Drawing.Size(292, 47);
      this.edStartAddress.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(238, 24);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(103, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "В пределах адреса";
      // 
      // cbStreet
      // 
      this.cbStreet.AutoSize = true;
      this.cbStreet.Location = new System.Drawing.Point(19, 70);
      this.cbStreet.Name = "cbStreet";
      this.cbStreet.Size = new System.Drawing.Size(58, 17);
      this.cbStreet.TabIndex = 2;
      this.cbStreet.Text = "Улица";
      this.cbStreet.UseVisualStyleBackColor = true;
      // 
      // cbPlanningStructure
      // 
      this.cbPlanningStructure.AutoSize = true;
      this.cbPlanningStructure.Location = new System.Drawing.Point(19, 47);
      this.cbPlanningStructure.Name = "cbPlanningStructure";
      this.cbPlanningStructure.Size = new System.Drawing.Size(158, 17);
      this.cbPlanningStructure.TabIndex = 1;
      this.cbPlanningStructure.Text = "Планировочная структура";
      this.cbPlanningStructure.UseVisualStyleBackColor = true;
      // 
      // cbVillage
      // 
      this.cbVillage.AutoSize = true;
      this.cbVillage.Location = new System.Drawing.Point(19, 24);
      this.cbVillage.Name = "cbVillage";
      this.cbVillage.Size = new System.Drawing.Size(121, 17);
      this.cbVillage.TabIndex = 0;
      this.cbVillage.Text = "Населенный пункт";
      this.cbVillage.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbActual);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Location = new System.Drawing.Point(0, 159);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(545, 52);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Параметры";
      // 
      // cbActual
      // 
      this.cbActual.AutoSize = true;
      this.cbActual.Location = new System.Drawing.Point(19, 28);
      this.cbActual.Name = "cbActual";
      this.cbActual.Size = new System.Drawing.Size(126, 17);
      this.cbActual.TabIndex = 0;
      this.cbActual.Text = "Только актуальные";
      this.cbActual.UseVisualStyleBackColor = true;
      // 
      // FiasSearchForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(545, 254);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FiasSearchForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Поиск адресного объекта";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbText;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox cbStreet;
    private System.Windows.Forms.CheckBox cbPlanningStructure;
    private System.Windows.Forms.CheckBox cbVillage;
    private System.Windows.Forms.TextBox edStartAddress;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox cbActual;
  }
}