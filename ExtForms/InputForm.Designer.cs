namespace FreeLibSet.Forms
{
  partial class InputForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnNo = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.MainLabel = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(8, 40);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnNo);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(149, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(104, 84);
      this.panel1.TabIndex = 1;
      // 
      // btnNo
      // 
      this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Location = new System.Drawing.Point(8, 70);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(88, 24);
      this.btnNo.TabIndex = 2;
      this.btnNo.Text = "Нет";
      this.btnNo.UseVisualStyleBackColor = true;
      this.btnNo.Visible = false;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.MainPanel);
      this.panel2.Controls.Add(this.MainLabel);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(149, 84);
      this.panel2.TabIndex = 0;
      // 
      // MainPanel
      // 
      this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.MainPanel.AutoSize = true;
      this.MainPanel.Location = new System.Drawing.Point(8, 38);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(132, 31);
      this.MainPanel.TabIndex = 1;
      // 
      // MainLabel
      // 
      this.MainLabel.AutoSize = true;
      this.MainLabel.Location = new System.Drawing.Point(9, 16);
      this.MainLabel.Name = "MainLabel";
      this.MainLabel.Size = new System.Drawing.Size(55, 13);
      this.MainLabel.TabIndex = 0;
      this.MainLabel.Text = "Значение";
      // 
      // InputForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(253, 84);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "InputForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Ввод текста";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    public System.Windows.Forms.Panel MainPanel;
    public System.Windows.Forms.Label MainLabel;
    public System.Windows.Forms.Button btnNo;
  }
}