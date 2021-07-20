namespace AgeyevAV.ExtForms.Docs
{
  partial class DocEditForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    public System.Windows.Forms.Panel ClientPanel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnApply;
    public System.Windows.Forms.Panel ButtonsPanel;
    public System.Windows.Forms.TabControl MainTabControl;
    private System.Windows.Forms.Button btnMore;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.ClientPanel = new System.Windows.Forms.Panel();
      this.ButtonsPanel = new System.Windows.Forms.Panel();
      this.btnMore = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.MainTabControl = new System.Windows.Forms.TabControl();
      this.ClientPanel.SuspendLayout();
      this.ButtonsPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // ClientPanel
      // 
      this.ClientPanel.Controls.Add(this.MainTabControl);
      this.ClientPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ClientPanel.Location = new System.Drawing.Point(0, 0);
      this.ClientPanel.Name = "ClientPanel";
      this.ClientPanel.Size = new System.Drawing.Size(393, 125);
      this.ClientPanel.TabIndex = 0;
      // 
      // ButtonsPanel
      // 
      this.ButtonsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.ButtonsPanel.Controls.Add(this.btnMore);
      this.ButtonsPanel.Controls.Add(this.btnApply);
      this.ButtonsPanel.Controls.Add(this.btnCancel);
      this.ButtonsPanel.Controls.Add(this.btnOK);
      this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.ButtonsPanel.Location = new System.Drawing.Point(0, 125);
      this.ButtonsPanel.Name = "ButtonsPanel";
      this.ButtonsPanel.Size = new System.Drawing.Size(393, 40);
      this.ButtonsPanel.TabIndex = 1;
      // 
      // btnMore
      // 
      this.btnMore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnMore.Location = new System.Drawing.Point(292, 9);
      this.btnMore.Name = "btnMore";
      this.btnMore.Size = new System.Drawing.Size(88, 24);
      this.btnMore.TabIndex = 4;
      this.btnMore.Text = "Ещ&ё";
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // btnApply
      // 
      this.btnApply.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnApply.Location = new System.Drawing.Point(198, 8);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(88, 24);
      this.btnApply.TabIndex = 3;
      this.btnApply.Text = "&Запись";
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnCancel.Location = new System.Drawing.Point(104, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // btnOK
      // 
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnOK.Location = new System.Drawing.Point(10, 8);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(88, 24);
      this.btnOK.TabIndex = 1;
      this.btnOK.Text = "О&К";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // MainTabControl
      // 
      this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainTabControl.Location = new System.Drawing.Point(0, 0);
      this.MainTabControl.Name = "MainTabControl";
      this.MainTabControl.SelectedIndex = 0;
      this.MainTabControl.ShowToolTips = true;
      this.MainTabControl.Size = new System.Drawing.Size(393, 125);
      this.MainTabControl.TabIndex = 2;
      // 
      // DocEditForm
      // 

      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AcceptButton = this.btnOK;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(393, 165);
      this.Controls.Add(this.ClientPanel);
      this.Controls.Add(this.ButtonsPanel);
      this.Name = "DocEditForm";
      this.ClientPanel.ResumeLayout(false);
      this.ButtonsPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
  }
}