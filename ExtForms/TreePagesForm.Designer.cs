namespace FreeLibSet.Forms
{
  partial class TreePagesForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.ThePanel = new System.Windows.Forms.Panel();
      this.TheTV = new System.Windows.Forms.TreeView();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 282);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(592, 40);
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
      // panel2
      // 
      this.panel2.Controls.Add(this.ThePanel);
      this.panel2.Controls.Add(this.TheTV);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(592, 282);
      this.panel2.TabIndex = 0;
      // 
      // ThePanel
      // 
      this.ThePanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ThePanel.Location = new System.Drawing.Point(178, 0);
      this.ThePanel.Name = "ThePanel";
      this.ThePanel.Size = new System.Drawing.Size(414, 282);
      this.ThePanel.TabIndex = 1;
      // 
      // TheTV
      // 
      this.TheTV.Dock = System.Windows.Forms.DockStyle.Left;
      this.TheTV.Location = new System.Drawing.Point(0, 0);
      this.TheTV.Name = "TheTV";
      this.TheTV.Size = new System.Drawing.Size(178, 282);
      this.TheTV.TabIndex = 0;
      // 
      // TreePagesForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(592, 322);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MinimizeBox = false;
      this.Name = "TreePagesForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel2;
    public System.Windows.Forms.Panel ThePanel;
    public System.Windows.Forms.TreeView TheTV;
  }
}