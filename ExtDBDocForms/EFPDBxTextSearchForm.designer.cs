namespace FreeLibSet.Forms.Docs
{
  partial class EFPDBxTextSearchForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPDBxTextSearchForm));
      this.btnDocSel = new System.Windows.Forms.Button();
      this.ButtonsPanel.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpWhere
      // 
      resources.ApplyResources(this.grpWhere, "grpWhere");
      // 
      // grpFrom
      // 
      resources.ApplyResources(this.grpFrom, "grpFrom");
      // 
      // grpDirection
      // 
      resources.ApplyResources(this.grpDirection, "grpDirection");
      // 
      // grpConditions
      // 
      resources.ApplyResources(this.grpConditions, "grpConditions");
      // 
      // ButtonsPanel
      // 
      resources.ApplyResources(this.ButtonsPanel, "ButtonsPanel");
      this.ButtonsPanel.Controls.Add(this.btnDocSel);
      this.ButtonsPanel.Controls.SetChildIndex(this.btnDocSel, 0);
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      // 
      // BottomPanel
      // 
      resources.ApplyResources(this.BottomPanel, "BottomPanel");
      // 
      // TopPanel
      // 
      resources.ApplyResources(this.TopPanel, "TopPanel");
      // 
      // btnDocSel
      // 
      resources.ApplyResources(this.btnDocSel, "btnDocSel");
      this.btnDocSel.Name = "btnDocSel";
      this.btnDocSel.UseVisualStyleBackColor = true;
      // 
      // EFPDBxTextSearchForm
      // 
      resources.ApplyResources(this, "$this");
      this.Name = "EFPDBxTextSearchForm";
      this.ButtonsPanel.ResumeLayout(false);
      this.MainPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    internal System.Windows.Forms.Button btnDocSel;
  }
}
