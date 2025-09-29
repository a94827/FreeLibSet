namespace WinFormsDemo.CloseFormDemo
{
  partial class TestCloseForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.btnAction = new System.Windows.Forms.Button();
      this.cbDialogResult = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.edDelay = new FreeLibSet.Controls.Int32EditBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbBanner = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 185);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(383, 40);
      this.panel1.TabIndex = 2;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(8, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnAction);
      this.groupBox1.Controls.Add(this.cbDialogResult);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(383, 91);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "EFPForm.CloseForm()";
      // 
      // btnAction
      // 
      this.btnAction.Location = new System.Drawing.Point(8, 54);
      this.btnAction.Name = "btnAction";
      this.btnAction.Size = new System.Drawing.Size(176, 24);
      this.btnAction.TabIndex = 2;
      this.btnAction.Text = "CloseForm()";
      this.btnAction.UseVisualStyleBackColor = true;
      // 
      // cbDialogResult
      // 
      this.cbDialogResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDialogResult.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDialogResult.FormattingEnabled = true;
      this.cbDialogResult.Location = new System.Drawing.Point(118, 27);
      this.cbDialogResult.Name = "cbDialogResult";
      this.cbDialogResult.Size = new System.Drawing.Size(253, 21);
      this.cbDialogResult.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "DialogResult";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.edDelay);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.cbBanner);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 91);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(383, 94);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Form activity";
      // 
      // edDelay
      // 
      this.edDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.edDelay.Increment = 1;
      this.edDelay.Location = new System.Drawing.Point(247, 55);
      this.edDelay.Maximum = 10;
      this.edDelay.Minimum = 0;
      this.edDelay.Name = "edDelay";
      this.edDelay.Size = new System.Drawing.Size(124, 20);
      this.edDelay.TabIndex = 2;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(14, 55);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(254, 20);
      this.label2.TabIndex = 1;
      this.label2.Text = "Closing delay with a splash (seconds)";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbBanner
      // 
      this.cbBanner.AutoSize = true;
      this.cbBanner.Location = new System.Drawing.Point(17, 26);
      this.cbBanner.Name = "cbBanner";
      this.cbBanner.Size = new System.Drawing.Size(335, 17);
      this.cbBanner.TabIndex = 0;
      this.cbBanner.Text = "Validation banner (prevents closing form when result is OK or Yes)";
      this.cbBanner.UseVisualStyleBackColor = true;
      // 
      // TestCloseForm
      // 
      this.AcceptButton = this.btnAction;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(383, 225);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "TestCloseForm";
      this.Text = "TestCloseForm";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnAction;
    private System.Windows.Forms.ComboBox cbDialogResult;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private FreeLibSet.Controls.Int32EditBox edDelay;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox cbBanner;
  }
}