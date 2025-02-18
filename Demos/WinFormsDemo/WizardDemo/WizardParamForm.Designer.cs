namespace WinFormsDemo.WizardDemo
{
  partial class WizardParamForm
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
      this.cbHelpContext = new System.Windows.Forms.CheckBox();
      this.cbSizeable = new System.Windows.Forms.CheckBox();
      this.cbImageKey = new System.Windows.Forms.CheckBox();
      this.cbConfigSectionName = new System.Windows.Forms.CheckBox();
      this.cbTitle = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 185);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(363, 49);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(136, 10);
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
      this.btnOk.Size = new System.Drawing.Size(117, 30);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
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
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Size = new System.Drawing.Size(363, 185);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Used Wizard properties";
      // 
      // cbHelpContext
      // 
      this.cbHelpContext.AutoSize = true;
      this.cbHelpContext.Location = new System.Drawing.Point(16, 150);
      this.cbHelpContext.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbHelpContext.Name = "cbHelpContext";
      this.cbHelpContext.Size = new System.Drawing.Size(106, 21);
      this.cbHelpContext.TabIndex = 4;
      this.cbHelpContext.Text = "HelpContext";
      this.cbHelpContext.UseVisualStyleBackColor = true;
      // 
      // cbSizeable
      // 
      this.cbSizeable.AutoSize = true;
      this.cbSizeable.Location = new System.Drawing.Point(16, 94);
      this.cbSizeable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbSizeable.Name = "cbSizeable";
      this.cbSizeable.Size = new System.Drawing.Size(84, 21);
      this.cbSizeable.TabIndex = 3;
      this.cbSizeable.Text = "Sizeable";
      this.cbSizeable.UseVisualStyleBackColor = true;
      // 
      // cbImageKey
      // 
      this.cbImageKey.AutoSize = true;
      this.cbImageKey.Location = new System.Drawing.Point(16, 63);
      this.cbImageKey.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbImageKey.Name = "cbImageKey";
      this.cbImageKey.Size = new System.Drawing.Size(92, 21);
      this.cbImageKey.TabIndex = 1;
      this.cbImageKey.Text = "ImageKey";
      this.cbImageKey.UseVisualStyleBackColor = true;
      // 
      // cbConfigSectionName
      // 
      this.cbConfigSectionName.AutoSize = true;
      this.cbConfigSectionName.Location = new System.Drawing.Point(16, 122);
      this.cbConfigSectionName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbConfigSectionName.Name = "cbConfigSectionName";
      this.cbConfigSectionName.Size = new System.Drawing.Size(154, 21);
      this.cbConfigSectionName.TabIndex = 2;
      this.cbConfigSectionName.Text = "ConfigSectionName";
      this.cbConfigSectionName.UseVisualStyleBackColor = true;
      // 
      // cbTitle
      // 
      this.cbTitle.AutoSize = true;
      this.cbTitle.Location = new System.Drawing.Point(16, 34);
      this.cbTitle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.cbTitle.Name = "cbTitle";
      this.cbTitle.Size = new System.Drawing.Size(57, 21);
      this.cbTitle.TabIndex = 0;
      this.cbTitle.Text = "Title";
      this.cbTitle.UseVisualStyleBackColor = true;
      // 
      // WizardParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(363, 234);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "WizardParamForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Wizard startup params";
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

