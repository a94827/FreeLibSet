namespace AgeyevAV.ExtForms.Docs
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.grpTree = new System.Windows.Forms.GroupBox();
      this.tvGroup = new AgeyevAV.ExtForms.TreeViewAdv();
      this.cbIncludeNestedGroups = new System.Windows.Forms.CheckBox();
      this.MainPanel.SuspendLayout();
      this.panel1.SuspendLayout();
      this.grpTree.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpTree);
      this.MainPanel.Controls.Add(this.panel1);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.cbIncludeNestedGroups);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 250);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(355, 37);
      this.panel1.TabIndex = 1;
      // 
      // grpTree
      // 
      this.grpTree.Controls.Add(this.tvGroup);
      this.grpTree.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpTree.Location = new System.Drawing.Point(0, 0);
      this.grpTree.Name = "grpTree";
      this.grpTree.Size = new System.Drawing.Size(355, 250);
      this.grpTree.TabIndex = 0;
      this.grpTree.TabStop = false;
      this.grpTree.Text = "Группа";
      // 
      // tvGroup
      // 
      this.tvGroup.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvGroup.ForeColor = System.Drawing.SystemColors.ControlText;
      this.tvGroup.Location = new System.Drawing.Point(3, 16);
      this.tvGroup.Name = "tvGroup";
      this.tvGroup.Size = new System.Drawing.Size(349, 231);
      this.tvGroup.TabIndex = 0;
      // 
      // cbIncludeNestedGroups
      // 
      this.cbIncludeNestedGroups.AutoSize = true;
      this.cbIncludeNestedGroups.Location = new System.Drawing.Point(8, 10);
      this.cbIncludeNestedGroups.Name = "cbIncludeNestedGroups";
      this.cbIncludeNestedGroups.Size = new System.Drawing.Size(170, 17);
      this.cbIncludeNestedGroups.TabIndex = 0;
      this.cbIncludeNestedGroups.Text = "Включая вложенные группы";
      this.cbIncludeNestedGroups.UseVisualStyleBackColor = true;
      // 
      // GroupGridFilterForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(355, 327);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
      this.MinimizeBox = false;
      this.Name = "GroupGridFilterForm";
      this.Text = "GroupGridFilterForm";
      this.MainPanel.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.grpTree.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox grpTree;
    private TreeViewAdv tvGroup;
    private System.Windows.Forms.CheckBox cbIncludeNestedGroups;
  }
}