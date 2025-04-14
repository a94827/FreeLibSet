namespace FreeLibSet.Forms.Docs
{
  partial class GroupGridFilterForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupGridFilterForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.cbIncludeNestedGroups = new System.Windows.Forms.CheckBox();
      this.grpTree = new System.Windows.Forms.GroupBox();
      this.tvGroup = new FreeLibSet.Controls.TreeViewAdv();
      this.MainPanel.SuspendLayout();
      this.panel1.SuspendLayout();
      this.grpTree.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      resources.ApplyResources(this.ButtonsPanel, "ButtonsPanel");
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpTree);
      this.MainPanel.Controls.Add(this.panel1);
      // 
      // BottomPanel
      // 
      resources.ApplyResources(this.BottomPanel, "BottomPanel");
      // 
      // TopPanel
      // 
      resources.ApplyResources(this.TopPanel, "TopPanel");
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.cbIncludeNestedGroups);
      this.panel1.Name = "panel1";
      // 
      // cbIncludeNestedGroups
      // 
      resources.ApplyResources(this.cbIncludeNestedGroups, "cbIncludeNestedGroups");
      this.cbIncludeNestedGroups.Name = "cbIncludeNestedGroups";
      this.cbIncludeNestedGroups.UseVisualStyleBackColor = true;
      // 
      // grpTree
      // 
      resources.ApplyResources(this.grpTree, "grpTree");
      this.grpTree.Controls.Add(this.tvGroup);
      this.grpTree.Name = "grpTree";
      this.grpTree.TabStop = false;
      // 
      // tvGroup
      // 
      resources.ApplyResources(this.tvGroup, "tvGroup");
      this.tvGroup.ForeColor = System.Drawing.SystemColors.ControlText;
      this.tvGroup.Name = "tvGroup";
      // 
      // GroupGridFilterForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
      this.Name = "GroupGridFilterForm";
      this.MainPanel.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.grpTree.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox grpTree;
    private FreeLibSet.Controls.TreeViewAdv tvGroup;
    private System.Windows.Forms.CheckBox cbIncludeNestedGroups;
  }
}
