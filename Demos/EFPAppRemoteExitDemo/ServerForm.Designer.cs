namespace EFPAppRemoteExitDemo
{
  partial class ServerForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnResume = new System.Windows.Forms.Button();
      this.btnExit = new System.Windows.Forms.Button();
      this.edMessage = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.lblClientStatus = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.btnResume);
      this.groupBox1.Controls.Add(this.btnExit);
      this.groupBox1.Controls.Add(this.edMessage);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(352, 134);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Управление клиентом";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 33);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(115, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Причина завершения";
      // 
      // btnResume
      // 
      this.btnResume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.btnResume.Location = new System.Drawing.Point(14, 105);
      this.btnResume.Name = "btnResume";
      this.btnResume.Size = new System.Drawing.Size(326, 24);
      this.btnResume.TabIndex = 3;
      this.btnResume.Text = "Разрешить продолжить работу";
      this.btnResume.UseVisualStyleBackColor = true;
      // 
      // btnExit
      // 
      this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.btnExit.Location = new System.Drawing.Point(14, 75);
      this.btnExit.Name = "btnExit";
      this.btnExit.Size = new System.Drawing.Size(326, 24);
      this.btnExit.TabIndex = 2;
      this.btnExit.Text = "Завершить";
      this.btnExit.UseVisualStyleBackColor = true;
      // 
      // edMessage
      // 
      this.edMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edMessage.Location = new System.Drawing.Point(14, 49);
      this.edMessage.Name = "edMessage";
      this.edMessage.Size = new System.Drawing.Size(326, 20);
      this.edMessage.TabIndex = 1;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.lblClientStatus);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 134);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(352, 75);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Состояние клиента";
      // 
      // lblClientStatus
      // 
      this.lblClientStatus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblClientStatus.Location = new System.Drawing.Point(3, 16);
      this.lblClientStatus.Name = "lblClientStatus";
      this.lblClientStatus.Size = new System.Drawing.Size(346, 56);
      this.lblClientStatus.TabIndex = 0;
      this.lblClientStatus.Text = "???";
      this.lblClientStatus.UseMnemonic = false;
      // 
      // ServerForm
      // 
      this.AcceptButton = this.btnExit;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(352, 209);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "ServerForm";
      this.Text = "Эиуляция окна сервера";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TextBox edMessage;
    private System.Windows.Forms.Button btnResume;
    private System.Windows.Forms.Button btnExit;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label lblClientStatus;
  }
}