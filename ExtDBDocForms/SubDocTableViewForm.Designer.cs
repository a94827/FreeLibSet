namespace FreeLibSet.Forms.Docs
{
  partial class SubDocTableViewForm
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
      this.TheButtonPanel = new System.Windows.Forms.Panel();
      this.TheNoButton = new System.Windows.Forms.Button();
      this.TheCancelButton = new System.Windows.Forms.Button();
      this.TheOKButton = new System.Windows.Forms.Button();
      this.ControlPanel = new System.Windows.Forms.Panel();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.TheButtonPanel.SuspendLayout();
      this.ControlPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // TheButtonPanel
      // 
      this.TheButtonPanel.Controls.Add(this.TheNoButton);
      this.TheButtonPanel.Controls.Add(this.TheCancelButton);
      this.TheButtonPanel.Controls.Add(this.TheOKButton);
      this.TheButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.TheButtonPanel.Location = new System.Drawing.Point(0, 362);
      this.TheButtonPanel.Name = "TheButtonPanel";
      this.TheButtonPanel.Size = new System.Drawing.Size(524, 40);
      this.TheButtonPanel.TabIndex = 5;
      this.TheButtonPanel.Visible = false;
      // 
      // TheNoButton
      // 
      this.TheNoButton.DialogResult = System.Windows.Forms.DialogResult.No;
      this.TheNoButton.Location = new System.Drawing.Point(198, 8);
      this.TheNoButton.Name = "TheNoButton";
      this.TheNoButton.Size = new System.Drawing.Size(88, 24);
      this.TheNoButton.TabIndex = 2;
      this.TheNoButton.Text = "&Нет";
      this.TheNoButton.UseVisualStyleBackColor = true;
      // 
      // TheCancelButton
      // 
      this.TheCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.TheCancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TheCancelButton.Location = new System.Drawing.Point(102, 8);
      this.TheCancelButton.Name = "TheCancelButton";
      this.TheCancelButton.Size = new System.Drawing.Size(88, 24);
      this.TheCancelButton.TabIndex = 1;
      this.TheCancelButton.Text = "Отмена";
      // 
      // TheOKButton
      // 
      this.TheOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.TheOKButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TheOKButton.Location = new System.Drawing.Point(8, 8);
      this.TheOKButton.Name = "TheOKButton";
      this.TheOKButton.Size = new System.Drawing.Size(88, 24);
      this.TheOKButton.TabIndex = 0;
      this.TheOKButton.Text = "О&К";
      // 
      // ControlPanel
      // 
      this.ControlPanel.Controls.Add(this.MainPanel);
      this.ControlPanel.Controls.Add(this.FilterGrid);
      this.ControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ControlPanel.Location = new System.Drawing.Point(0, 0);
      this.ControlPanel.Name = "ControlPanel";
      this.ControlPanel.Size = new System.Drawing.Size(524, 362);
      this.ControlPanel.TabIndex = 6;
      // 
      // FilterGrid
      // 
      this.FilterGrid.Dock = System.Windows.Forms.DockStyle.Top;
      this.FilterGrid.Enabled = false;
      this.FilterGrid.Location = new System.Drawing.Point(0, 0);
      this.FilterGrid.Name = "FilterGrid";
      this.FilterGrid.Size = new System.Drawing.Size(524, 33);
      this.FilterGrid.TabIndex = 5;
      this.FilterGrid.Visible = false;
      // 
      // MainPanel
      // 
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 33);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(524, 329);
      this.MainPanel.TabIndex = 7;
      // 
      // SubDocTableViewForm
      // 
      this.AcceptButton = this.TheOKButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.TheCancelButton;
      this.ClientSize = new System.Drawing.Size(524, 402);
      this.Controls.Add(this.ControlPanel);
      this.Controls.Add(this.TheButtonPanel);
      this.Name = "SubDocTableViewForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
      this.Text = "aaa";
      this.TheButtonPanel.ResumeLayout(false);
      this.ControlPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    internal System.Windows.Forms.Panel TheButtonPanel;
    internal System.Windows.Forms.Button TheNoButton;
    internal System.Windows.Forms.Button TheCancelButton;
    internal System.Windows.Forms.Button TheOKButton;
    internal System.Windows.Forms.Panel ControlPanel;
    internal System.Windows.Forms.DataGridView FilterGrid;
    internal System.Windows.Forms.Panel MainPanel;
  }
}