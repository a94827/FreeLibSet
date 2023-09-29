namespace WizardDemo
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbImageKey = new System.Windows.Forms.CheckBox();
      this.cbConfigSectionName = new System.Windows.Forms.CheckBox();
      this.cbTitle = new System.Windows.Forms.CheckBox();
      this.cbSizeable = new System.Windows.Forms.CheckBox();
      this.cbHelpContext = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 150);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(272, 40);
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
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbHelpContext);
      this.groupBox1.Controls.Add(this.cbSizeable);
      this.groupBox1.Controls.Add(this.cbImageKey);
      this.groupBox1.Controls.Add(this.cbConfigSectionName);
      this.groupBox1.Controls.Add(this.cbTitle);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(272, 150);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры Wizard";
      // 
      // cbImageKey
      // 
      this.cbImageKey.AutoSize = true;
      this.cbImageKey.Location = new System.Drawing.Point(12, 51);
      this.cbImageKey.Name = "cbImageKey";
      this.cbImageKey.Size = new System.Drawing.Size(73, 17);
      this.cbImageKey.TabIndex = 1;
      this.cbImageKey.Text = "ImageKey";
      this.cbImageKey.UseVisualStyleBackColor = true;
      // 
      // cbConfigSectionName
      // 
      this.cbConfigSectionName.AutoSize = true;
      this.cbConfigSectionName.Location = new System.Drawing.Point(12, 99);
      this.cbConfigSectionName.Name = "cbConfigSectionName";
      this.cbConfigSectionName.Size = new System.Drawing.Size(120, 17);
      this.cbConfigSectionName.TabIndex = 2;
      this.cbConfigSectionName.Text = "ConfigSectionName";
      this.cbConfigSectionName.UseVisualStyleBackColor = true;
      // 
      // cbTitle
      // 
      this.cbTitle.AutoSize = true;
      this.cbTitle.Location = new System.Drawing.Point(12, 28);
      this.cbTitle.Name = "cbTitle";
      this.cbTitle.Size = new System.Drawing.Size(46, 17);
      this.cbTitle.TabIndex = 0;
      this.cbTitle.Text = "Title";
      this.cbTitle.UseVisualStyleBackColor = true;
      // 
      // cbSizeable
      // 
      this.cbSizeable.AutoSize = true;
      this.cbSizeable.Location = new System.Drawing.Point(12, 76);
      this.cbSizeable.Name = "cbSizeable";
      this.cbSizeable.Size = new System.Drawing.Size(66, 17);
      this.cbSizeable.TabIndex = 3;
      this.cbSizeable.Text = "Sizeable";
      this.cbSizeable.UseVisualStyleBackColor = true;
      // 
      // cbHelpContext
      // 
      this.cbHelpContext.AutoSize = true;
      this.cbHelpContext.Location = new System.Drawing.Point(12, 122);
      this.cbHelpContext.Name = "cbHelpContext";
      this.cbHelpContext.Size = new System.Drawing.Size(84, 17);
      this.cbHelpContext.TabIndex = 4;
      this.cbHelpContext.Text = "HelpContext";
      this.cbHelpContext.UseVisualStyleBackColor = true;
      // 
      // ParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(272, 190);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ParamForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Параметры запуска мастера";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox cbConfigSectionName;
    private System.Windows.Forms.CheckBox cbTitle;
    private System.Windows.Forms.CheckBox cbImageKey;
    private System.Windows.Forms.CheckBox cbHelpContext;
    private System.Windows.Forms.CheckBox cbSizeable;
  }
}

