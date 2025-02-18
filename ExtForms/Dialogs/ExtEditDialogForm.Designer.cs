namespace FreeLibSet.Forms
{
  partial class ExtEditDialogForm
  {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtEditDialogForm));
      this.ClientPanel = new System.Windows.Forms.Panel();
      this.MainTabControl = new System.Windows.Forms.TabControl();
      this.ButtonsPanel = new System.Windows.Forms.Panel();
      this.btnMore = new System.Windows.Forms.Button();
      this.btnApply = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOK = new System.Windows.Forms.Button();
      this.ClientPanel.SuspendLayout();
      this.ButtonsPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // ClientPanel
      // 
      resources.ApplyResources(this.ClientPanel, "ClientPanel");
      this.ClientPanel.Controls.Add(this.MainTabControl);
      this.ClientPanel.Name = "ClientPanel";
      // 
      // MainTabControl
      // 
      resources.ApplyResources(this.MainTabControl, "MainTabControl");
      this.MainTabControl.Name = "MainTabControl";
      this.MainTabControl.SelectedIndex = 0;
      // 
      // ButtonsPanel
      // 
      resources.ApplyResources(this.ButtonsPanel, "ButtonsPanel");
      this.ButtonsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.ButtonsPanel.Controls.Add(this.btnMore);
      this.ButtonsPanel.Controls.Add(this.btnApply);
      this.ButtonsPanel.Controls.Add(this.btnCancel);
      this.ButtonsPanel.Controls.Add(this.btnOK);
      this.ButtonsPanel.Name = "ButtonsPanel";
      // 
      // btnMore
      // 
      resources.ApplyResources(this.btnMore, "btnMore");
      this.btnMore.Name = "btnMore";
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // btnApply
      // 
      resources.ApplyResources(this.btnApply, "btnApply");
      this.btnApply.Name = "btnApply";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      // 
      // btnOK
      // 
      resources.ApplyResources(this.btnOK, "btnOK");
      this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOK.Name = "btnOK";
      // 
      // ExtEditDialogForm
      // 
      this.AcceptButton = this.btnOK;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.ClientPanel);
      this.Controls.Add(this.ButtonsPanel);
      this.Name = "ExtEditDialogForm";
      this.ClientPanel.ResumeLayout(false);
      this.ButtonsPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
  }
}
