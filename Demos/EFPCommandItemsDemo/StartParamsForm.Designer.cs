namespace EFPCommandItemsDemo
{
  partial class StartParamsForm
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
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbMDI = new System.Windows.Forms.RadioButton();
      this.rbSDI = new System.Windows.Forms.RadioButton();
      this.cbDebugWindow = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 133);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(229, 40);
      this.panel1.TabIndex = 1;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.cbDebugWindow);
      this.panel2.Controls.Add(this.groupBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(229, 133);
      this.panel2.TabIndex = 0;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(94, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(80, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbSDI);
      this.groupBox1.Controls.Add(this.rbMDI);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(200, 81);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Интерфейс";
      // 
      // rbMDI
      // 
      this.rbMDI.AutoSize = true;
      this.rbMDI.Location = new System.Drawing.Point(16, 26);
      this.rbMDI.Name = "rbMDI";
      this.rbMDI.Size = new System.Drawing.Size(45, 17);
      this.rbMDI.TabIndex = 0;
      this.rbMDI.TabStop = true;
      this.rbMDI.Text = "MDI";
      this.rbMDI.UseVisualStyleBackColor = true;
      // 
      // rbSDI
      // 
      this.rbSDI.AutoSize = true;
      this.rbSDI.Location = new System.Drawing.Point(16, 49);
      this.rbSDI.Name = "rbSDI";
      this.rbSDI.Size = new System.Drawing.Size(43, 17);
      this.rbSDI.TabIndex = 1;
      this.rbSDI.TabStop = true;
      this.rbSDI.Text = "SDI";
      this.rbSDI.UseVisualStyleBackColor = true;
      // 
      // cbDebugWindow
      // 
      this.cbDebugWindow.AutoSize = true;
      this.cbDebugWindow.Location = new System.Drawing.Point(28, 99);
      this.cbDebugWindow.Name = "cbDebugWindow";
      this.cbDebugWindow.Size = new System.Drawing.Size(113, 17);
      this.cbDebugWindow.TabIndex = 1;
      this.cbDebugWindow.Text = "Отладочное окно";
      this.cbDebugWindow.UseVisualStyleBackColor = true;
      // 
      // StartParamsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(229, 173);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StartParamsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Параметры запуска";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.CheckBox cbDebugWindow;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbSDI;
    private System.Windows.Forms.RadioButton rbMDI;
  }
}