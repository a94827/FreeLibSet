using System.Windows.Forms;
namespace FreeLibSet.Forms.Docs
{
  partial class DocTableViewForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocTableViewForm));
      this.TheSplitter = new System.Windows.Forms.SplitContainer();
      this.panel2 = new System.Windows.Forms.Panel();
      this.GroupTree = new FreeLibSet.Controls.TreeViewAdv();
      this.GroupSpeedPanel = new System.Windows.Forms.Panel();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.TheButtonPanel = new System.Windows.Forms.Panel();
      this.TheNoButton = new System.Windows.Forms.Button();
      this.TheCancelButton = new System.Windows.Forms.Button();
      this.TheOKButton = new System.Windows.Forms.Button();
      this.ControlPanel = new System.Windows.Forms.Panel();
      this.GroupCBPanel = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.GroupCB = new System.Windows.Forms.ComboBox();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.TheSplitter.Panel1.SuspendLayout();
      this.TheSplitter.Panel2.SuspendLayout();
      this.TheSplitter.SuspendLayout();
      this.panel2.SuspendLayout();
      this.TheButtonPanel.SuspendLayout();
      this.ControlPanel.SuspendLayout();
      this.GroupCBPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // TheSplitter
      // 
      resources.ApplyResources(this.TheSplitter, "TheSplitter");
      this.TheSplitter.Name = "TheSplitter";
      // 
      // TheSplitter.Panel1
      // 
      resources.ApplyResources(this.TheSplitter.Panel1, "TheSplitter.Panel1");
      this.TheSplitter.Panel1.Controls.Add(this.panel2);
      // 
      // TheSplitter.Panel2
      // 
      resources.ApplyResources(this.TheSplitter.Panel2, "TheSplitter.Panel2");
      this.TheSplitter.Panel2.Controls.Add(this.MainPanel);
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.GroupTree);
      this.panel2.Controls.Add(this.GroupSpeedPanel);
      this.panel2.Name = "panel2";
      // 
      // GroupTree
      // 
      resources.ApplyResources(this.GroupTree, "GroupTree");
      this.GroupTree.ForeColor = System.Drawing.SystemColors.ControlText;
      this.GroupTree.Name = "GroupTree";
      // 
      // GroupSpeedPanel
      // 
      resources.ApplyResources(this.GroupSpeedPanel, "GroupSpeedPanel");
      this.GroupSpeedPanel.Name = "GroupSpeedPanel";
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Name = "MainPanel";
      // 
      // TheButtonPanel
      // 
      resources.ApplyResources(this.TheButtonPanel, "TheButtonPanel");
      this.TheButtonPanel.Controls.Add(this.TheNoButton);
      this.TheButtonPanel.Controls.Add(this.TheCancelButton);
      this.TheButtonPanel.Controls.Add(this.TheOKButton);
      this.TheButtonPanel.Name = "TheButtonPanel";
      // 
      // TheNoButton
      // 
      resources.ApplyResources(this.TheNoButton, "TheNoButton");
      this.TheNoButton.DialogResult = System.Windows.Forms.DialogResult.No;
      this.TheNoButton.Name = "TheNoButton";
      this.TheNoButton.UseVisualStyleBackColor = true;
      this.TheNoButton.Click += new System.EventHandler(this.FormNoButton_Click);
      // 
      // TheCancelButton
      // 
      resources.ApplyResources(this.TheCancelButton, "TheCancelButton");
      this.TheCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.TheCancelButton.Name = "TheCancelButton";
      this.TheCancelButton.Click += new System.EventHandler(this.FormCancelButton_Click);
      // 
      // TheOKButton
      // 
      resources.ApplyResources(this.TheOKButton, "TheOKButton");
      this.TheOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.TheOKButton.Name = "TheOKButton";
      this.TheOKButton.Click += new System.EventHandler(this.FormOKButton_Click);
      // 
      // ControlPanel
      // 
      resources.ApplyResources(this.ControlPanel, "ControlPanel");
      this.ControlPanel.Controls.Add(this.TheSplitter);
      this.ControlPanel.Controls.Add(this.GroupCBPanel);
      this.ControlPanel.Controls.Add(this.FilterGrid);
      this.ControlPanel.Name = "ControlPanel";
      // 
      // GroupCBPanel
      // 
      resources.ApplyResources(this.GroupCBPanel, "GroupCBPanel");
      this.GroupCBPanel.Controls.Add(this.label1);
      this.GroupCBPanel.Controls.Add(this.GroupCB);
      this.GroupCBPanel.Name = "GroupCBPanel";
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // GroupCB
      // 
      resources.ApplyResources(this.GroupCB, "GroupCB");
      this.GroupCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.GroupCB.FormattingEnabled = true;
      this.GroupCB.Name = "GroupCB";
      // 
      // FilterGrid
      // 
      resources.ApplyResources(this.FilterGrid, "FilterGrid");
      this.FilterGrid.Name = "FilterGrid";
      // 
      // DocTableViewForm
      // 
      this.AcceptButton = this.TheOKButton;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.TheCancelButton;
      this.Controls.Add(this.ControlPanel);
      this.Controls.Add(this.TheButtonPanel);
      this.Name = "DocTableViewForm";
      this.TheSplitter.Panel1.ResumeLayout(false);
      this.TheSplitter.Panel2.ResumeLayout(false);
      this.TheSplitter.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.TheButtonPanel.ResumeLayout(false);
      this.ControlPanel.ResumeLayout(false);
      this.GroupCBPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private Panel TheButtonPanel;
    private Button TheOKButton;
    private Button TheCancelButton;
    private Button TheNoButton;
    internal SplitContainer TheSplitter;
    private Panel panel2;
    internal FreeLibSet.Controls.TreeViewAdv GroupTree;
    internal Panel GroupSpeedPanel;
    internal Panel MainPanel;
    internal Panel GroupCBPanel;
    private Label label1;
    internal ComboBox GroupCB;
    internal DataGridView FilterGrid;
    internal Panel ControlPanel;
  }
}
