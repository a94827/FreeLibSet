namespace AgeyevAV.ExtForms
{
  partial class EFPAppRemoteExitForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPAppRemoteExitForm));
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.messageBoxIconBox1 = new AgeyevAV.ExtForms.MessageBoxIconBox();
      this.grpMessage = new System.Windows.Forms.GroupBox();
      this.edMessage = new System.Windows.Forms.TextBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.label4 = new System.Windows.Forms.Label();
      this.lblTime = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.ThePB = new System.Windows.Forms.ProgressBar();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.grpMessage.SuspendLayout();
      this.panel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.messageBoxIconBox1);
      this.groupBox1.Controls.Add(this.grpMessage);
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Location = new System.Drawing.Point(10, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(396, 265);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(35, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(358, 111);
      this.label1.TabIndex = 4;
      this.label1.Text = resources.GetString("label1.Text");
      // 
      // messageBoxIconBox1
      // 
      this.messageBoxIconBox1.Dock = System.Windows.Forms.DockStyle.Left;
      this.messageBoxIconBox1.Icon = System.Windows.Forms.MessageBoxIcon.Warning;
      this.messageBoxIconBox1.IconSize = AgeyevAV.ExtForms.MessageBoxIconSize.Large;
      this.messageBoxIconBox1.Location = new System.Drawing.Point(3, 16);
      this.messageBoxIconBox1.Name = "messageBoxIconBox1";
      this.messageBoxIconBox1.Size = new System.Drawing.Size(32, 111);
      this.messageBoxIconBox1.TabIndex = 3;
      // 
      // grpMessage
      // 
      this.grpMessage.Controls.Add(this.edMessage);
      this.grpMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.grpMessage.Location = new System.Drawing.Point(3, 127);
      this.grpMessage.Name = "grpMessage";
      this.grpMessage.Size = new System.Drawing.Size(390, 71);
      this.grpMessage.TabIndex = 1;
      this.grpMessage.TabStop = false;
      this.grpMessage.Text = "Сообщение";
      // 
      // edMessage
      // 
      this.edMessage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edMessage.Location = new System.Drawing.Point(3, 16);
      this.edMessage.Multiline = true;
      this.edMessage.Name = "edMessage";
      this.edMessage.ReadOnly = true;
      this.edMessage.Size = new System.Drawing.Size(384, 52);
      this.edMessage.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.label4);
      this.panel1.Controls.Add(this.lblTime);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(3, 198);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(390, 26);
      this.panel1.TabIndex = 2;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(352, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(21, 21);
      this.label4.TabIndex = 2;
      this.label4.Text = "с.";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblTime
      // 
      this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblTime.Location = new System.Drawing.Point(299, 0);
      this.lblTime.Name = "lblTime";
      this.lblTime.Size = new System.Drawing.Size(47, 21);
      this.lblTime.TabIndex = 1;
      this.lblTime.Text = "???";
      this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(300, 21);
      this.label2.TabIndex = 0;
      this.label2.Text = "Попытка закрытия программы будет выполнена через";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.ThePB);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox2.Location = new System.Drawing.Point(3, 224);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(390, 38);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      // 
      // ThePB
      // 
      this.ThePB.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ThePB.Location = new System.Drawing.Point(3, 16);
      this.ThePB.Name = "ThePB";
      this.ThePB.Size = new System.Drawing.Size(384, 19);
      this.ThePB.TabIndex = 0;
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnClose.Location = new System.Drawing.Point(16, 277);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(176, 24);
      this.btnClose.TabIndex = 1;
      this.btnClose.Text = "Закрыть сейчас";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(224, 277);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(176, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // EFPAppRemoteExitForm
      // 
      this.AcceptButton = this.btnClose;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(416, 303);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPAppRemoteExitForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Завершение работы";
      this.groupBox1.ResumeLayout(false);
      this.grpMessage.ResumeLayout(false);
      this.grpMessage.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label lblTime;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ProgressBar ThePB;
    private System.Windows.Forms.TextBox edMessage;
    private System.Windows.Forms.Label label1;
    private MessageBoxIconBox messageBoxIconBox1;
    public System.Windows.Forms.GroupBox grpMessage;
  }
}