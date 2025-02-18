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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialogForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.grpSets = new System.Windows.Forms.GroupBox();
      this.SetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.AuxPanel = new System.Windows.Forms.Panel();
      this.ButtonPanel = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.panel1.SuspendLayout();
      this.grpSets.SuspendLayout();
      this.ButtonPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.AccessibleDescription = null;
      this.panel1.AccessibleName = null;
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.BackgroundImage = null;
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.AuxPanel);
      this.panel1.Controls.Add(this.ButtonPanel);
      this.panel1.Font = null;
      this.panel1.Name = "panel1";
      // 
      // grpSets
      // 
      this.grpSets.AccessibleDescription = null;
      this.grpSets.AccessibleName = null;
      resources.ApplyResources(this.grpSets, "grpSets");
      this.grpSets.BackgroundImage = null;
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Font = null;
      this.grpSets.Name = "grpSets";
      this.grpSets.TabStop = false;
      // 
      // SetComboBox
      // 
      this.SetComboBox.AccessibleDescription = null;
      this.SetComboBox.AccessibleName = null;
      resources.ApplyResources(this.SetComboBox, "SetComboBox");
      this.SetComboBox.BackgroundImage = null;
      this.SetComboBox.Font = null;
      this.SetComboBox.MinimumSize = new System.Drawing.Size(200, 24);
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.ShowMD5 = false;
      // 
      // AuxPanel
      // 
      this.AuxPanel.AccessibleDescription = null;
      this.AuxPanel.AccessibleName = null;
      resources.ApplyResources(this.AuxPanel, "AuxPanel");
      this.AuxPanel.BackgroundImage = null;
      this.AuxPanel.Font = null;
      this.AuxPanel.Name = "AuxPanel";
      // 
      // ButtonPanel
      // 
      this.ButtonPanel.AccessibleDescription = null;
      this.ButtonPanel.AccessibleName = null;
      resources.ApplyResources(this.ButtonPanel, "ButtonPanel");
      this.ButtonPanel.BackgroundImage = null;
      this.ButtonPanel.Controls.Add(this.btnCancel);
      this.ButtonPanel.Controls.Add(this.btnOk);
      this.ButtonPanel.Font = null;
      this.ButtonPanel.Name = "ButtonPanel";
      // 
      // btnCancel
      // 
      this.btnCancel.AccessibleDescription = null;
      this.btnCancel.AccessibleName = null;
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.BackgroundImage = null;
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Font = null;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.AccessibleDescription = null;
      this.btnOk.AccessibleName = null;
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.BackgroundImage = null;
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Font = null;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // TheTabControl
      // 
      this.TheTabControl.AccessibleDescription = null;
      this.TheTabControl.AccessibleName = null;
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.BackgroundImage = null;
      this.TheTabControl.Font = null;
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // SettingsDialogForm
      // 
      this.AcceptButton = this.btnOk;
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Font = null;
      this.Icon = null;
      this.Name = "SettingsDialogForm";
      this.panel1.ResumeLayout(false);
      this.grpSets.ResumeLayout(false);
      this.ButtonPanel.ResumeLayout(false);
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
