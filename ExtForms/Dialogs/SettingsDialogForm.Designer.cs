namespace FreeLibSet.Forms
{
  partial class SettingsDialogForm
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
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.ButtonPanel = new System.Windows.Forms.Panel();
      this.AuxPanel = new System.Windows.Forms.Panel();
      this.grpSets = new System.Windows.Forms.GroupBox();
      this.SetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.ButtonPanel.SuspendLayout();
      this.grpSets.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.AuxPanel);
      this.panel1.Controls.Add(this.ButtonPanel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 402);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 48);
      this.panel1.TabIndex = 0;
      // 
      // TheTabControl
      // 
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(632, 402);
      this.TheTabControl.TabIndex = 1;
      // 
      // ButtonPanel
      // 
      this.ButtonPanel.Controls.Add(this.btnCancel);
      this.ButtonPanel.Controls.Add(this.btnOk);
      this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Left;
      this.ButtonPanel.Location = new System.Drawing.Point(0, 0);
      this.ButtonPanel.Name = "ButtonPanel";
      this.ButtonPanel.Size = new System.Drawing.Size(200, 48);
      this.ButtonPanel.TabIndex = 0;
      // 
      // AuxPanel
      // 
      this.AuxPanel.Dock = System.Windows.Forms.DockStyle.Left;
      this.AuxPanel.Location = new System.Drawing.Point(200, 0);
      this.AuxPanel.Name = "AuxPanel";
      this.AuxPanel.Size = new System.Drawing.Size(59, 48);
      this.AuxPanel.TabIndex = 1;
      this.AuxPanel.Visible = false;
      // 
      // grpSets
      // 
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpSets.Location = new System.Drawing.Point(259, 0);
      this.grpSets.Name = "grpSets";
      this.grpSets.Size = new System.Drawing.Size(373, 48);
      this.grpSets.TabIndex = 3;
      this.grpSets.TabStop = false;
      this.grpSets.Text = "Готовые наборы";
      // 
      // SetComboBox
      // 
      this.SetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.SetComboBox.Location = new System.Drawing.Point(6, 17);
      this.SetComboBox.MinimumSize = new System.Drawing.Size(200, 24);
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.Size = new System.Drawing.Size(361, 24);
      this.SetComboBox.TabIndex = 2;
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
      // SettingsDialogForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(632, 450);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Name = "SettingsDialogForm";
      this.AcceptButton = btnOk;
      this.CancelButton = btnCancel;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.panel1.ResumeLayout(false);
      this.ButtonPanel.ResumeLayout(false);
      this.grpSets.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel AuxPanel;
    private System.Windows.Forms.Panel ButtonPanel;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.GroupBox grpSets;
    private FreeLibSet.Controls.ParamSetComboBox SetComboBox;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
  }
}
