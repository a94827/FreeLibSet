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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowExceptionMsgBoxForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnMore = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.MsgLabel = new System.Windows.Forms.Label();
      this.panel3 = new System.Windows.Forms.Panel();
      this.messageBoxIconBox1 = new FreeLibSet.Controls.MessageBoxIconBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnMore);
      this.panel1.Controls.Add(this.btnOk);
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Name = "panel1";
      // 
      // btnMore
      // 
      resources.ApplyResources(this.btnMore, "btnMore");
      this.btnMore.Name = "btnMore";
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.SystemColors.Window;
      this.panel2.Controls.Add(this.MsgLabel);
      this.panel2.Controls.Add(this.panel3);
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.ForeColor = System.Drawing.SystemColors.WindowText;
      this.panel2.Name = "panel2";
      // 
      // MsgLabel
      // 
      resources.ApplyResources(this.MsgLabel, "MsgLabel");
      this.MsgLabel.Name = "MsgLabel";
      this.MsgLabel.UseMnemonic = false;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.messageBoxIconBox1);
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Name = "panel3";
      // 
      // messageBoxIconBox1
      // 
      this.messageBoxIconBox1.Icon = System.Windows.Forms.MessageBoxIcon.Hand;
      this.messageBoxIconBox1.IconSize = FreeLibSet.Controls.MessageBoxIconSize.Large;
      resources.ApplyResources(this.messageBoxIconBox1, "messageBoxIconBox1");
      this.messageBoxIconBox1.Name = "messageBoxIconBox1";
      // 
      // ShowExceptionMsgBoxForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnOk;
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
