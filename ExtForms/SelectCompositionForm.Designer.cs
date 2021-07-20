namespace AgeyevAV.ExtForms
{
  partial class SelectCompositionForm
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
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbParamSet = new AgeyevAV.ExtForms.ParamSetComboBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.lblInfo = new AgeyevAV.ExtForms.InfoLabel();
      this.pbPreview = new System.Windows.Forms.PictureBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnXml = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.groupBox2);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 397);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(624, 45);
      this.panel1.TabIndex = 1;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbParamSet);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(200, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(424, 45);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Сохраненные композиции";
      // 
      // cbParamSet
      // 
      this.cbParamSet.Location = new System.Drawing.Point(6, 17);
      this.cbParamSet.MinimumSize = new System.Drawing.Size(200, 24);
      this.cbParamSet.Name = "cbParamSet";
      this.cbParamSet.SelectedCode = "";
      this.cbParamSet.SelectedItem = null;
      this.cbParamSet.SelectedMD5Sum = "";
      this.cbParamSet.Size = new System.Drawing.Size(415, 24);
      this.cbParamSet.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Controls.Add(this.btnOk);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(200, 45);
      this.panel2.TabIndex = 0;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 17);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 17);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.lblInfo);
      this.groupBox1.Controls.Add(this.pbPreview);
      this.groupBox1.Controls.Add(this.panel3);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(624, 397);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Выбранная композиция";
      // 
      // lblInfo
      // 
      this.lblInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblInfo.IconSize = AgeyevAV.ExtForms.MessageBoxIconSize.Large;
      this.lblInfo.Location = new System.Drawing.Point(3, 16);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(578, 378);
      this.lblInfo.TabIndex = 5;
      this.lblInfo.Text = "???";
      // 
      // pbPreview
      // 
      this.pbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbPreview.Location = new System.Drawing.Point(3, 16);
      this.pbPreview.Name = "pbPreview";
      this.pbPreview.Size = new System.Drawing.Size(578, 378);
      this.pbPreview.TabIndex = 4;
      this.pbPreview.TabStop = false;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnXml);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel3.Location = new System.Drawing.Point(581, 16);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(40, 378);
      this.panel3.TabIndex = 3;
      // 
      // btnXml
      // 
      this.btnXml.Location = new System.Drawing.Point(8, 8);
      this.btnXml.Name = "btnXml";
      this.btnXml.Size = new System.Drawing.Size(24, 24);
      this.btnXml.TabIndex = 0;
      this.btnXml.UseVisualStyleBackColor = true;
      // 
      // SelectCompositionForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(624, 442);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Name = "SelectCompositionForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Композиции рабочего стола";
      this.panel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
      this.panel3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private ParamSetComboBox cbParamSet;
    private InfoLabel lblInfo;
    private System.Windows.Forms.PictureBox pbPreview;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnXml;
  }
}