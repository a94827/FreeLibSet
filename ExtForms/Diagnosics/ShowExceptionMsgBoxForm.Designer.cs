namespace FreeLibSet.Forms.Diagnostics
{
  partial class ShowExceptionMsgBoxForm
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
      this.panel2 = new System.Windows.Forms.Panel();
      this.panel3 = new System.Windows.Forms.Panel();
      this.messageBoxIconBox1 = new FreeLibSet.Controls.MessageBoxIconBox();
      this.MsgLabel = new System.Windows.Forms.Label();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnMore = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnMore);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 148);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(463, 40);
      this.panel1.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.MsgLabel);
      this.panel2.Controls.Add(this.panel3);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(463, 148);
      this.panel2.BackColor = System.Drawing.SystemColors.Window;
      this.panel2.ForeColor = System.Drawing.SystemColors.WindowText;
      this.panel2.TabIndex = 1;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.messageBoxIconBox1);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(64, 148);
      this.panel3.TabIndex = 0;
      // 
      // messageBoxIconBox1
      // 
      this.messageBoxIconBox1.Icon = System.Windows.Forms.MessageBoxIcon.Hand;
      this.messageBoxIconBox1.IconSize = FreeLibSet.Controls.MessageBoxIconSize.Large;
      this.messageBoxIconBox1.Location = new System.Drawing.Point(16, 58);
      this.messageBoxIconBox1.Name = "messageBoxIconBox1";
      this.messageBoxIconBox1.Size = new System.Drawing.Size(32, 32);
      this.messageBoxIconBox1.TabIndex = 0;
      // 
      // MsgLabel
      // 
      this.MsgLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MsgLabel.Location = new System.Drawing.Point(64, 0);
      this.MsgLabel.Name = "MsgLabel";
      this.MsgLabel.Size = new System.Drawing.Size(399, 148);
      this.MsgLabel.TabIndex = 1;
      this.MsgLabel.Text = "???";
      this.MsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.MsgLabel.UseMnemonic = false;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(187, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnMore
      // 
      this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMore.Location = new System.Drawing.Point(319, 8);
      this.btnMore.Name = "btnMore";
      this.btnMore.Size = new System.Drawing.Size(132, 24);
      this.btnMore.TabIndex = 1;
      this.btnMore.Text = "Подробности";
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // ShowExceptionMsgBoxForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOk;
      this.ClientSize = new System.Drawing.Size(463, 188);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ShowExceptionMsgBoxForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnMore;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panel3;
    private FreeLibSet.Controls.MessageBoxIconBox messageBoxIconBox1;
    public System.Windows.Forms.Label MsgLabel;
  }
}